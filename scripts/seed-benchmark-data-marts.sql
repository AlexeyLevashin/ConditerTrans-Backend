-- Однократное создание синтетического набора в отдельной схеме.
-- Не запускайте повторно: CREATE SCHEMA тогда остановится без изменения данных.
-- После создания замените в benchmark-data-marts.sql
-- SET search_path TO public; на SET search_path TO mart_benchmark, public;
-- В этом наборе 100 000 заказов, ~200 000 строк заказа,
-- 12 500 отказов и несколько тысяч строк рейтинга.
-- В рабочие таблицы приложения данные не записываются.

BEGIN;
SET LOCAL TIME ZONE 'UTC';
CREATE SCHEMA mart_benchmark;

CREATE TABLE mart_benchmark.orders (
    id uuid PRIMARY KEY,
    creation_date timestamptz NOT NULL,
    status integer NOT NULL
);
CREATE INDEX ix_benchmark_orders_creation_date
    ON mart_benchmark.orders (creation_date);

CREATE TABLE mart_benchmark.products (
    id uuid PRIMARY KEY,
    company_id uuid NOT NULL,
    name varchar(256) NOT NULL
);
CREATE INDEX ix_benchmark_products_company_id
    ON mart_benchmark.products (company_id);

CREATE TABLE mart_benchmark.order_lines (
    id uuid PRIMARY KEY,
    order_id uuid NOT NULL REFERENCES mart_benchmark.orders(id),
    product_id uuid NOT NULL REFERENCES mart_benchmark.products(id)
);
CREATE INDEX ix_benchmark_order_lines_order_id
    ON mart_benchmark.order_lines (order_id);

CREATE TABLE mart_benchmark.order_change_histories (
    id uuid PRIMARY KEY,
    order_id uuid NOT NULL REFERENCES mart_benchmark.orders(id),
    change_time timestamptz NOT NULL,
    order_status integer NOT NULL,
    rejection_reason_id uuid
);
CREATE INDEX ix_benchmark_history_status_time
    ON mart_benchmark.order_change_histories (order_status, change_time);

CREATE TABLE mart_benchmark.dispatcher_rejection_facts (
    order_id uuid PRIMARY KEY REFERENCES mart_benchmark.orders(id),
    company_id uuid NOT NULL,
    rejected_at timestamptz NOT NULL,
    rejection_reason_id uuid NOT NULL
);
CREATE INDEX ix_benchmark_rejection_company_time
    ON mart_benchmark.dispatcher_rejection_facts (company_id, rejected_at);

CREATE TABLE mart_benchmark.dispatcher_product_rating_daily (
    order_date date NOT NULL,
    company_id uuid NOT NULL,
    product_name varchar(256) NOT NULL,
    order_count integer NOT NULL,
    PRIMARY KEY (company_id, order_date, product_name)
);

-- 4 компании с 40 продуктами. Первая использует тот же ID, что в запросах.
INSERT INTO mart_benchmark.products (id, company_id, name)
SELECT md5('product-' || company_index || '-' || product_index)::uuid,
       CASE WHEN company_index = 0
           THEN '1b0b8bcf-61c9-44c6-9150-f73da30ce21c'::uuid
           ELSE md5('company-' || company_index)::uuid END,
       'Продукт ' || lpad(product_index::text, 2, '0')
FROM generate_series(0, 3) AS company_index
CROSS JOIN generate_series(0, 39) AS product_index;

-- Равномерно распределяем даты за последние 90 дней и все 8 статусов.
-- Компания меняется раз в 8 заказов, поэтому каждая имеет все статусы.
INSERT INTO mart_benchmark.orders (id, creation_date, status)
SELECT md5('order-' || order_number)::uuid,
       (CURRENT_DATE - (order_number % 90))::timestamptz
           + (order_number % 24) * interval '1 hour',
       order_number % 8
FROM generate_series(1, 100000) AS order_number;

-- От одной до трёх строк на заказ, только продукты его компании.
INSERT INTO mart_benchmark.order_lines (id, order_id, product_id)
SELECT md5('line-' || order_number || '-' || line_number)::uuid,
       md5('order-' || order_number)::uuid,
       md5('product-' || (((order_number - 1) / 8) % 4)
           || '-' || ((order_number + line_number * 7) % 40))::uuid
FROM generate_series(1, 100000) AS order_number
CROSS JOIN LATERAL generate_series(1, (order_number % 3) + 1) AS line_number;

-- Одно событие создания на заказ и отдельное событие отказа для статуса 4.
INSERT INTO mart_benchmark.order_change_histories
    (id, order_id, change_time, order_status, rejection_reason_id)
SELECT md5('created-' || o.id)::uuid, o.id, o.creation_date, 1, NULL
FROM mart_benchmark.orders o;

INSERT INTO mart_benchmark.order_change_histories
    (id, order_id, change_time, order_status, rejection_reason_id)
SELECT md5('rejected-' || o.id)::uuid, o.id,
       o.creation_date + interval '2 hours', 4,
       '00000000-0000-0000-0000-000000000001'::uuid
FROM mart_benchmark.orders o
WHERE o.status = 4;

INSERT INTO mart_benchmark.dispatcher_rejection_facts
    (order_id, company_id, rejected_at, rejection_reason_id)
SELECT o.id, p.company_id, o.creation_date + interval '2 hours',
       '00000000-0000-0000-0000-000000000001'::uuid
FROM mart_benchmark.orders o
JOIN mart_benchmark.order_lines ol ON ol.order_id = o.id
JOIN mart_benchmark.products p ON p.id = ol.product_id
WHERE o.status = 4
GROUP BY o.id, p.company_id, o.creation_date;

INSERT INTO mart_benchmark.dispatcher_product_rating_daily
    (order_date, company_id, product_name, order_count)
SELECT (o.creation_date AT TIME ZONE 'UTC')::date,
       p.company_id, p.name, count(DISTINCT o.id)::integer
FROM mart_benchmark.orders o
JOIN mart_benchmark.order_lines ol ON ol.order_id = o.id
JOIN mart_benchmark.products p ON p.id = ol.product_id
WHERE o.status IN (1, 2, 3, 5, 6, 7)
  AND NOT EXISTS (
      SELECT 1
      FROM mart_benchmark.order_lines other_line
      LEFT JOIN mart_benchmark.products other_product
          ON other_product.id = other_line.product_id
      WHERE other_line.order_id = o.id
        AND (other_product.id IS NULL OR other_product.company_id <> p.company_id)
  )
GROUP BY (o.creation_date AT TIME ZONE 'UTC')::date, p.company_id, p.name;

ANALYZE mart_benchmark.orders;
ANALYZE mart_benchmark.products;
ANALYZE mart_benchmark.order_lines;
ANALYZE mart_benchmark.order_change_histories;
ANALYZE mart_benchmark.dispatcher_rejection_facts;
ANALYZE mart_benchmark.dispatcher_product_rating_daily;
COMMIT;

-- Проверка объёма данных и непустой выборки для компании из benchmark-data-marts.sql.
SELECT 'orders' AS source, count(*) FROM mart_benchmark.orders
UNION ALL SELECT 'order_lines', count(*) FROM mart_benchmark.order_lines
UNION ALL SELECT 'rejection_facts', count(*) FROM mart_benchmark.dispatcher_rejection_facts
UNION ALL SELECT 'product_rating', count(*) FROM mart_benchmark.dispatcher_product_rating_daily
UNION ALL SELECT 'selected_company_rejections', count(*)
FROM mart_benchmark.dispatcher_rejection_facts
WHERE company_id = '1b0b8bcf-61c9-44c6-9150-f73da30ce21c'
  AND rejected_at >= (CURRENT_DATE - 29)::timestamptz
  AND rejected_at < (CURRENT_DATE + 1)::timestamptz;
