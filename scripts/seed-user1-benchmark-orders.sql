-- Тестовые заказы для диспетчера user1@example.com.
-- Запускайте целиком в DataGrip на локальной учебной базе после всех миграций.
-- Требуются минимум 6 товаров компании 1b0b8bcf-61c9-44c6-9150-f73da30ce21c
-- (при необходимости сначала выполните seed-company-products.sql).
-- Скрипт добавляет 1200 заказов за последние 90 дней, 2–3 строки в каждом;
-- часть заказов отклонена и попадает в витрину отказов.
-- Идентификаторы фиксированы: повторный запуск не добавляет дубликаты.
-- Скрипт не меняет существующие заказы и профиль пользователя.
-- Закрываем транзакцию, оставшуюся после предыдущей неудачной попытки.

ROLLBACK;
BEGIN;
SET LOCAL TIME ZONE 'UTC';

CREATE TEMP TABLE seed_benchmark_user (
    dispatcher_id uuid NOT NULL,
    manager_id uuid
) ON COMMIT DROP;

INSERT INTO seed_benchmark_user
SELECT u.id, (
    SELECT manager.id
    FROM public.users manager
    JOIN public.employees manager_employee ON manager_employee.id = manager.employee_id
    JOIN public.companies buyer ON buyer.id = manager_employee.company_id
    WHERE manager.role = 0 AND buyer.company_type = 0
    ORDER BY (lower(manager.email) = 'user@example.com') DESC, manager.id
    LIMIT 1
)
FROM public.users u
JOIN public.employees e ON e.id = u.employee_id
JOIN public.companies c ON c.id = e.company_id
WHERE lower(u.email) = 'user1@example.com'
  AND u.role = 1  -- Dispatcher
  AND c.company_type = 1
  AND c.id = '1b0b8bcf-61c9-44c6-9150-f73da30ce21c';

DO $$
BEGIN
    IF (SELECT count(*) FROM seed_benchmark_user) <> 1 THEN
        RAISE EXCEPTION 'user1@example.com должен быть диспетчером указанной производственной компании, а в базе нужен хотя бы один менеджер компании-заказчика';
    END IF;
    IF (SELECT manager_id FROM seed_benchmark_user) IS NULL THEN
        RAISE EXCEPTION 'Не найден менеджер компании-заказчика для создания заказов';
    END IF;
    IF NOT EXISTS (
        SELECT 1 FROM public.companies
        WHERE id = '1b0b8bcf-61c9-44c6-9150-f73da30ce21c'
          AND company_type = 1
    ) THEN
        RAISE EXCEPTION 'Указанная производственная компания не найдена';
    END IF;
    IF (SELECT count(*) FROM public.products
        WHERE company_id = '1b0b8bcf-61c9-44c6-9150-f73da30ce21c') < 6 THEN
        RAISE EXCEPTION 'Для производственной компании нужно минимум 6 товаров';
    END IF;
    IF NOT EXISTS (SELECT 1 FROM public.rejection_reasons
                   WHERE id = '10000000-0000-0000-0000-000000000001') THEN
        RAISE EXCEPTION 'Не найдена стандартная причина отказа; проверьте миграции';
    END IF;
    IF to_regprocedure('public.refresh_dispatcher_product_rating_daily()') IS NULL THEN
        RAISE EXCEPTION 'Сначала примените миграцию таблицы рейтинга продукции';
    END IF;
END $$;

CREATE TEMP TABLE seed_benchmark_products ON COMMIT DROP AS
SELECT id, row_number() OVER (ORDER BY name, id)::integer AS product_no
FROM (
    SELECT id, name FROM public.products
    WHERE company_id = '1b0b8bcf-61c9-44c6-9150-f73da30ce21c'
    ORDER BY name, id LIMIT 12
) products;

CREATE TEMP TABLE seed_benchmark_orders ON COMMIT DROP AS
SELECT n AS sequence_no,
       md5('user1-mart-benchmark-v1:order:' || n)::uuid AS id,
       (CURRENT_DATE - (n % 90))::timestamptz
           + (n % 20) * interval '1 hour' AS creation_date,
       CASE WHEN n % 8 = 0 THEN 4  -- Rejected
            WHEN n % 8 IN (1, 5) THEN 1  -- PendingApproval
            WHEN n % 8 IN (2, 6) THEN 3  -- Rescheduled
            ELSE 2 END AS status,  -- Confirmed
       (SELECT COALESCE(max(order_number), 0) FROM public.orders) + n AS order_number
FROM generate_series(1, 1200) AS series(n);

INSERT INTO public.orders
    (id, order_number, creation_date, status, manager_id, dispatcher_id, cargo_id,
     production_address, delivery_address, payment_type, requested_delivery_date,
     deadline_confirmation_phase)
SELECT o.id, o.order_number, o.creation_date, o.status, u.manager_id,
       u.dispatcher_id, NULL,
       'Демо: адрес погрузки производства',
       'Демо: адрес доставки для user1',
       'Безналичный расчёт', o.creation_date + interval '7 days', 0
FROM seed_benchmark_orders o CROSS JOIN seed_benchmark_user u
ON CONFLICT (id) DO NOTHING;

INSERT INTO public.order_lines (id, order_id, product_id, quantity_of_units)
SELECT md5('user1-mart-benchmark-v1:line:' || o.sequence_no || ':' || line_no)::uuid,
       o.id, p.id, 1 + ((o.sequence_no + line_no) % 10)
FROM seed_benchmark_orders o
CROSS JOIN LATERAL generate_series(1, 2 + (o.sequence_no % 2)) AS line_item(line_no)
JOIN seed_benchmark_products p ON p.product_no =
    ((o.sequence_no * 7 + line_no * 11) % (SELECT count(*) FROM seed_benchmark_products)) + 1
ON CONFLICT (id) DO NOTHING;

-- События отказа и факты имеют одинаковую дату, чтобы сравнение 1А/1Б
-- возвращало одинаковый результат.
INSERT INTO public.order_change_histories
    (id, order_id, change_time, order_status, comment, rejection_reason_id)
SELECT md5('user1-mart-benchmark-v1:created:' || o.sequence_no)::uuid,
       o.id, o.creation_date, 1, 'Тестовый заказ', NULL
FROM seed_benchmark_orders o
ON CONFLICT (id) DO NOTHING;

INSERT INTO public.order_change_histories
    (id, order_id, change_time, order_status, comment, rejection_reason_id)
SELECT md5('user1-mart-benchmark-v1:final:' || o.sequence_no)::uuid,
       o.id, o.creation_date + interval '2 hours', o.status,
       'Тестовый статус для сравнения витрин',
       CASE WHEN o.status = 4
           THEN '10000000-0000-0000-0000-000000000001'::uuid
           ELSE NULL END
FROM seed_benchmark_orders o
WHERE o.status <> 1
ON CONFLICT (id) DO NOTHING;

INSERT INTO public.dispatcher_rejection_facts
    (order_id, company_id, rejected_at, rejection_reason_id)
SELECT o.id, '1b0b8bcf-61c9-44c6-9150-f73da30ce21c'::uuid,
       o.creation_date + interval '2 hours',
       '10000000-0000-0000-0000-000000000001'::uuid
FROM seed_benchmark_orders o WHERE o.status = 4
ON CONFLICT (order_id) DO NOTHING;

SELECT public.refresh_dispatcher_product_rating_daily();
COMMIT;

-- Проверка: заказы назначены диспетчеру user1, и обе витрины непусты.
SELECT 'user1_orders' AS source, count(*) AS row_count
FROM public.orders o JOIN public.users u ON u.id = o.dispatcher_id
WHERE u.email = 'user1@example.com'
  AND o.id IN (SELECT md5('user1-mart-benchmark-v1:order:' || n)::uuid
               FROM generate_series(1, 1200) AS series(n))
UNION ALL
SELECT 'rejection_facts', count(*) FROM public.dispatcher_rejection_facts
WHERE order_id IN (SELECT md5('user1-mart-benchmark-v1:order:' || n)::uuid
                   FROM generate_series(1, 1200) AS series(n))
UNION ALL
SELECT 'rating_rows_last_30_days', count(*)
FROM public.dispatcher_product_rating_daily
WHERE company_id = '1b0b8bcf-61c9-44c6-9150-f73da30ce21c'
  AND order_date >= CURRENT_DATE - 29;
