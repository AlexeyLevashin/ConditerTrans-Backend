-- Запустите в SQL-консоли DataGrip для той же PostgreSQL-базы, где работает API.
-- Для реальных данных оставьте public; для отдельного набора из
-- seed-benchmark-data-marts.sql измените строку ниже на mart_benchmark, public.
-- Укажите компанию и период во ВСЕХ четырёх запросах.
-- Ни один запрос не изменяет данные. Период: 30 дней, включая сегодня, в UTC.
-- Для недели замените 29 на 6 в каждом вхождении ниже.
-- В каждом плане сравнивайте Execution Time (мс), actual rows и Buffers.
-- Повторите пары несколько раз после прогрева кэша и сравните медианы.
SET TIME ZONE 'UTC';
SET search_path TO public;

-- Проверьте, есть ли данные для выбранной компании и периода.
SELECT 'rejection_fact' AS mart, count(*) AS rows_in_period
FROM dispatcher_rejection_facts
WHERE company_id = '1b0b8bcf-61c9-44c6-9150-f73da30ce21c'
  AND rejected_at >= (CURRENT_DATE - 29)::timestamptz
  AND rejected_at < (CURRENT_DATE + 1)::timestamptz
UNION ALL
SELECT 'product_rating', count(*)
FROM dispatcher_product_rating_daily
WHERE company_id = '1b0b8bcf-61c9-44c6-9150-f73da30ce21c'
  AND order_date >= CURRENT_DATE - 29
  AND order_date < CURRENT_DATE + 1;

-- 1А. Аналитический запрос к витрине отказов: число отказов по дням.
EXPLAIN (ANALYZE, BUFFERS)
SELECT (f.rejected_at AT TIME ZONE 'UTC')::date AS report_date,
       count(*)::integer AS rejection_count
FROM dispatcher_rejection_facts f
WHERE f.company_id = '1b0b8bcf-61c9-44c6-9150-f73da30ce21c'
  AND f.rejected_at >= (CURRENT_DATE - 29)::timestamptz
  AND f.rejected_at < (CURRENT_DATE + 1)::timestamptz
GROUP BY (f.rejected_at AT TIME ZONE 'UTC')::date
ORDER BY report_date;

-- 1Б. Тот же результат из оперативных таблиц: последнее событие отказа
-- на заказ плюс компания продукта, как при заполнении витрины.
EXPLAIN (ANALYZE, BUFFERS)
WITH last_rejection AS (
    SELECT DISTINCT ON (h.order_id)
           h.order_id,
           h.change_time AS rejected_at
    FROM order_change_histories h
    WHERE h.order_status = 4
      AND h.rejection_reason_id IS NOT NULL
    ORDER BY h.order_id, h.change_time DESC, h.id DESC
)
SELECT (r.rejected_at AT TIME ZONE 'UTC')::date AS report_date,
       count(*)::integer AS rejection_count
FROM last_rejection r
JOIN LATERAL (
    SELECT p.company_id
    FROM order_lines ol
    JOIN products p ON p.id = ol.product_id
    WHERE ol.order_id = r.order_id
    LIMIT 1
) product_company ON true
WHERE product_company.company_id = '1b0b8bcf-61c9-44c6-9150-f73da30ce21c'
  AND r.rejected_at >= (CURRENT_DATE - 29)::timestamptz
  AND r.rejected_at < (CURRENT_DATE + 1)::timestamptz
GROUP BY (r.rejected_at AT TIME ZONE 'UTC')::date
ORDER BY report_date;

-- 2А. Аналитический запрос к таблице рейтинга: день, продукт, число заказов.
EXPLAIN (ANALYZE, BUFFERS)
SELECT m.order_date AS report_date, m.product_name, m.order_count
FROM dispatcher_product_rating_daily m
WHERE m.company_id = '1b0b8bcf-61c9-44c6-9150-f73da30ce21c'
  AND m.order_date >= CURRENT_DATE - 29
  AND m.order_date < CURRENT_DATE + 1
ORDER BY m.order_date, m.order_count DESC, m.product_name;

-- 2Б. Тот же результат из оперативных таблиц. Предикат и агрегация
-- совпадают с функцией refresh_dispatcher_product_rating_daily().
EXPLAIN (ANALYZE, BUFFERS)
SELECT (o.creation_date AT TIME ZONE 'UTC')::date AS report_date,
       p.name AS product_name,
       count(DISTINCT o.id)::integer AS order_count
FROM orders o
JOIN order_lines ol ON ol.order_id = o.id
JOIN products p ON p.id = ol.product_id
WHERE p.company_id = '1b0b8bcf-61c9-44c6-9150-f73da30ce21c'
  AND o.creation_date >= (CURRENT_DATE - 29)::timestamptz
  AND o.creation_date < (CURRENT_DATE + 1)::timestamptz
  AND o.status IN (1, 2, 3, 5, 6, 7)
  AND NOT EXISTS (
      SELECT 1
      FROM order_lines other_line
      LEFT JOIN products other_product ON other_product.id = other_line.product_id
      WHERE other_line.order_id = o.id
        AND (other_product.id IS NULL OR other_product.company_id <> p.company_id)
  )
GROUP BY (o.creation_date AT TIME ZONE 'UTC')::date, p.name
ORDER BY report_date, order_count DESC, product_name;
