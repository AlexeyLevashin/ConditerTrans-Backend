-- Демонстрационные данные для лабораторной работы по иерархиям.
-- Запускать в базе API после всех миграций, включая AddCategoryHierarchy.
-- Добавляет 20 подкатегорий к «Кондитерские изделия», 5 производств,
-- 200 товаров, 600 заказов и 2100 строк заказа.
-- Если корневой категории «Кондитерские изделия» нет, создаёт и её.
-- Дополнительно создаёт технического покупателя, сотрудника и менеджера для FK orders.manager_id.
-- Данные вымышленные: наименования товаров похожи на реальные, ИНН и контакты не принадлежат компаниям.
-- Повторный запуск не создаёт дубликаты. Скрипт не обновляет существующие строки.
-- Запускайте весь файл целиком в одной SQL-сессии.
-- Первая команда отменяет незавершённую транзакцию в этой сессии, если она есть.

ROLLBACK;

BEGIN;

-- После частичного запуска временные таблицы могут остаться в той же сессии.
DROP TABLE IF EXISTS pg_temp.seed_orders;
DROP TABLE IF EXISTS pg_temp.seed_products;
DROP TABLE IF EXISTS pg_temp.seed_producers;
DROP TABLE IF EXISTS pg_temp.seed_categories;
DROP TABLE IF EXISTS pg_temp.seed_catalog_root;

INSERT INTO public.categories (id, name, parent_id, path)
SELECT md5('hierarchy-demo-v1:catalog-root')::uuid,
       'Кондитерские изделия',
       NULL,
       text2ltree('c' || md5('hierarchy-demo-v1:catalog-root'))
WHERE NOT EXISTS (
    SELECT 1 FROM public.categories
    WHERE name = 'Кондитерские изделия' AND parent_id IS NULL
)
ON CONFLICT (id) DO NOTHING;

CREATE TEMP TABLE seed_catalog_root (id uuid NOT NULL, path ltree NOT NULL)
ON COMMIT DROP;

INSERT INTO seed_catalog_root (id, path)
SELECT id, path FROM public.categories
WHERE name = 'Кондитерские изделия' AND parent_id IS NULL;

DO $$
BEGIN
    IF (SELECT count(*) FROM seed_catalog_root) <> 1 THEN
        RAISE EXCEPTION 'Ожидалась ровно одна корневая категория «Кондитерские изделия»';
    END IF;
END $$;

CREATE TEMP TABLE seed_categories (
    category_key text PRIMARY KEY,
    parent_key text,
    category_name text NOT NULL
) ON COMMIT DROP;

INSERT INTO seed_categories (category_key, parent_key, category_name) VALUES
    ('confectionery', NULL, 'Пряничные и вафельные изделия'),
    ('gingerbread', 'confectionery', 'Пряники'),
    ('glazed_gingerbread', 'gingerbread', 'Пряники глазированные'),
    ('waffles', 'confectionery', 'Вафли'),
    ('chocolate_waffles', 'waffles', 'Вафли шоколадные'),
    ('bakery', NULL, 'Выпечка'),
    ('cookies', 'bakery', 'Печенье'),
    ('oat_cookies', 'cookies', 'Печенье овсяное'),
    ('muffins', 'bakery', 'Кексы'),
    ('filled_muffins', 'muffins', 'Кексы с начинкой'),
    ('desserts', NULL, 'Торты и пирожные'),
    ('cakes', 'desserts', 'Торты'),
    ('honey_cakes', 'cakes', 'Медовые торты'),
    ('pastries', 'desserts', 'Пирожные'),
    ('eclairs', 'pastries', 'Эклеры'),
    ('sweets', NULL, 'Сладости'),
    ('marshmallow', 'sweets', 'Зефир'),
    ('chocolate_marshmallow', 'marshmallow', 'Зефир в шоколаде'),
    ('candies', 'sweets', 'Конфеты'),
    ('chocolate_candies', 'candies', 'Шоколадные конфеты');

WITH RECURSIVE category_tree AS (
    SELECT c.category_key, c.category_name, c.parent_key,
           md5('hierarchy-demo-v1:category:' || c.category_key)::uuid AS id,
           root.id AS parent_id,
           root.path || text2ltree('c' || md5('hierarchy-demo-v1:category:' || c.category_key)) AS path
    FROM seed_categories c
    CROSS JOIN seed_catalog_root root
    WHERE c.parent_key IS NULL
    UNION ALL
    SELECT c.category_key, c.category_name, c.parent_key,
           md5('hierarchy-demo-v1:category:' || c.category_key)::uuid,
           parent.id,
           parent.path || text2ltree('c' || md5('hierarchy-demo-v1:category:' || c.category_key))
    FROM seed_categories c
    JOIN category_tree parent ON parent.category_key = c.parent_key
)
INSERT INTO public.categories (id, name, parent_id, path)
SELECT id, category_name, parent_id, path FROM category_tree
WHERE true
ON CONFLICT (id) DO NOTHING;

-- Если старая версия этого скрипта уже создала четыре отдельные корневые
-- ветки, переносим каждую вместе со всеми потомками под общий корень.
DO $$
DECLARE
    root_id uuid;
    root_path ltree;
    branch_key text;
    branch_id uuid;
    old_path ltree;
    old_parent_id uuid;
    new_prefix ltree;
BEGIN
    SELECT id, path INTO root_id, root_path FROM seed_catalog_root;

    FOR branch_key IN
        SELECT category_key FROM seed_categories WHERE parent_key IS NULL
    LOOP
        branch_id := md5('hierarchy-demo-v1:category:' || branch_key)::uuid;
        SELECT path, parent_id INTO old_path, old_parent_id
        FROM public.categories WHERE id = branch_id;

        IF old_parent_id IS DISTINCT FROM root_id THEN
            new_prefix := root_path || subpath(old_path, nlevel(old_path) - 1, 1);
            UPDATE public.categories c
            SET path = CASE
                           WHEN c.id = branch_id THEN new_prefix
                           ELSE new_prefix || subpath(c.path, nlevel(old_path))
                       END,
                parent_id = CASE
                                WHEN c.id = branch_id THEN root_id
                                ELSE c.parent_id
                            END
            WHERE c.path <@ old_path;
        END IF;
    END LOOP;
END $$;

CREATE TEMP TABLE seed_producers (
    producer_no integer PRIMARY KEY,
    brand text NOT NULL,
    company_name text NOT NULL,
    city text NOT NULL,
    address text NOT NULL
) ON COMMIT DROP;

INSERT INTO seed_producers VALUES
    (1, 'Северная пекарня', 'Демо-производство «Северная пекарня»', 'Санкт-Петербург', 'Санкт-Петербург, ул. Полевая, 12'),
    (2, 'Коломенские сладости', 'Демо-производство «Коломенские сладости»', 'Коломна', 'Коломна, ул. Лесная, 8'),
    (3, 'Волжский десерт', 'Демо-производство «Волжский десерт»', 'Казань', 'Казань, ул. Заводская, 21'),
    (4, 'Тульская выпечка', 'Демо-производство «Тульская выпечка»', 'Тула', 'Тула, ул. Хлебная, 5'),
    (5, 'Кубанская кондитерская', 'Демо-производство «Кубанская кондитерская»', 'Краснодар', 'Краснодар, ул. Садовая, 17');

INSERT INTO public.companies
    (id, inn, email, phone, name, address, description, created_at, company_type)
SELECT md5('hierarchy-demo-v1:producer:' || producer_no)::uuid,
       '990000000' || lpad(producer_no::text, 3, '0'),
       'seed-producer-' || producer_no || '@example.invalid',
       '+7-900-000-00-' || lpad(producer_no::text, 2, '0'),
       company_name, address,
       'Учебное производство. Названия и реквизиты вымышлены.',
       '2026-01-01 09:00:00+03'::timestamptz, 1
FROM seed_producers
WHERE true
ON CONFLICT (id) DO NOTHING;

-- Технический покупатель нужен только для корректной связи orders.manager_id.
INSERT INTO public.companies
    (id, inn, email, phone, name, address, description, created_at, company_type)
VALUES (
    md5('hierarchy-demo-v1:buyer')::uuid, '990000000099',
    'seed-buyer@example.invalid', '+7-900-000-00-99',
    'Демо-закупки', 'Москва, ул. Складская, 10',
    'Технический покупатель для учебных заказов.',
    '2026-01-01 09:00:00+03'::timestamptz, 0
)
ON CONFLICT (id) DO NOTHING;

INSERT INTO public.employees
    (id, phone, employee_number, surname, name, patronymic, created_at, company_id)
VALUES (
    md5('hierarchy-demo-v1:manager-employee')::uuid,
    '+7-900-000-00-98', 'SEED-MANAGER-1', 'Тестовый', 'Менеджер', NULL,
    '2026-01-01 09:00:00+03'::timestamptz,
    md5('hierarchy-demo-v1:buyer')::uuid
)
ON CONFLICT (id) DO NOTHING;

INSERT INTO public.users (id, email, is_admin, password_hash, role, employee_id)
VALUES (
    md5('hierarchy-demo-v1:manager-user')::uuid,
    'seed-manager@example.invalid', false, '', 0,
    md5('hierarchy-demo-v1:manager-employee')::uuid
)
ON CONFLICT (id) DO NOTHING;

CREATE TEMP TABLE seed_products (
    product_no integer PRIMARY KEY,
    category_key text NOT NULL,
    product_name text NOT NULL,
    price numeric(12, 2) NOT NULL,
    quantity real NOT NULL,
    expiry real NOT NULL,
    units_of_measure integer NOT NULL
) ON COMMIT DROP;

-- 40 конкретных позиций × 5 производителей = 200 товаров.
-- quantity: масса упаковки в граммах (unit=1) либо число штук (unit=0).
INSERT INTO seed_products VALUES
    (1, 'gingerbread', 'Пряники заварные с абрикосовым повидлом, 350 г', 190, 350, 45, 1),
    (2, 'gingerbread', 'Пряники тульские с яблочной начинкой, 400 г', 240, 400, 45, 1),
    (3, 'gingerbread', 'Пряники мятные классические, 300 г', 165, 300, 60, 1),
    (4, 'glazed_gingerbread', 'Пряники глазированные с вишней, 350 г', 225, 350, 45, 1),
    (5, 'glazed_gingerbread', 'Пряники глазированные с шоколадом, 350 г', 235, 350, 45, 1),
    (6, 'waffles', 'Вафли сливочные с ванильным кремом, 250 г', 155, 250, 120, 1),
    (7, 'waffles', 'Вафли ореховые с пралине, 250 г', 175, 250, 120, 1),
    (8, 'waffles', 'Вафли хрустящие с карамелью, 200 г', 145, 200, 120, 1),
    (9, 'chocolate_waffles', 'Вафли шоколадные с фундуком, 250 г', 195, 250, 120, 1),
    (10, 'chocolate_waffles', 'Вафли в тёмной глазури, 200 г', 185, 200, 120, 1),
    (11, 'cookies', 'Печенье песочное с лимонной цедрой, 300 г', 165, 300, 90, 1),
    (12, 'cookies', 'Печенье сахарное с корицей, 300 г', 155, 300, 90, 1),
    (13, 'cookies', 'Печенье с кусочками шоколада, 250 г', 185, 250, 90, 1),
    (14, 'oat_cookies', 'Печенье овсяное с изюмом, 300 г', 175, 300, 60, 1),
    (15, 'oat_cookies', 'Печенье овсяное с клюквой, 300 г', 185, 300, 60, 1),
    (16, 'muffins', 'Кекс столичный с изюмом, 400 г', 260, 400, 21, 1),
    (17, 'muffins', 'Кекс мраморный ванильно-шоколадный, 400 г', 285, 400, 21, 1),
    (18, 'muffins', 'Кекс лимонный с сахарной глазурью, 350 г', 275, 350, 21, 1),
    (19, 'filled_muffins', 'Маффин с черничной начинкой, 120 г', 110, 120, 10, 1),
    (20, 'filled_muffins', 'Маффин шоколадный с кремом, 120 г', 120, 120, 10, 1),
    (21, 'cakes', 'Торт бисквитный с клубникой, 900 г', 1150, 900, 5, 1),
    (22, 'cakes', 'Торт шоколадный с вишней, 1000 г', 1350, 1000, 5, 1),
    (23, 'cakes', 'Торт Наполеон с заварным кремом, 950 г', 1250, 950, 5, 1),
    (24, 'honey_cakes', 'Торт Медовик со сметанным кремом, 900 г', 1100, 900, 5, 1),
    (25, 'honey_cakes', 'Торт медовый с черносливом, 850 г', 1180, 850, 5, 1),
    (26, 'pastries', 'Пирожное Картошка, 100 г', 95, 100, 5, 1),
    (27, 'pastries', 'Пирожное корзиночка с ягодами, 120 г', 135, 120, 3, 1),
    (28, 'pastries', 'Пирожное бисквитное с малиной, 120 г', 145, 120, 5, 1),
    (29, 'eclairs', 'Эклер с ванильным заварным кремом, 90 г', 125, 90, 3, 1),
    (30, 'eclairs', 'Эклер с шоколадным кремом, 90 г', 135, 90, 3, 1),
    (31, 'marshmallow', 'Зефир яблочный ванильный, 250 г', 210, 250, 60, 1),
    (32, 'marshmallow', 'Зефир с чёрной смородиной, 250 г', 225, 250, 60, 1),
    (33, 'marshmallow', 'Зефир с малиной, 250 г', 225, 250, 60, 1),
    (34, 'chocolate_marshmallow', 'Зефир в тёмном шоколаде, 200 г', 255, 200, 60, 1),
    (35, 'chocolate_marshmallow', 'Зефир в молочном шоколаде, 200 г', 245, 200, 60, 1),
    (36, 'candies', 'Конфеты помадные сливочные, 300 г', 295, 300, 120, 1),
    (37, 'candies', 'Конфеты с мягкой карамелью, 300 г', 310, 300, 120, 1),
    (38, 'candies', 'Конфеты с грильяжем, 250 г', 330, 250, 120, 1),
    (39, 'chocolate_candies', 'Конфеты шоколадные с фундуком, 250 г', 390, 250, 120, 1),
    (40, 'chocolate_candies', 'Конфеты шоколадные с вишней, 250 г', 410, 250, 120, 1);

INSERT INTO public.products
    (id, name, description, price, quantity, expiry, units_of_measure, category_id, company_id, file_id)
SELECT md5('hierarchy-demo-v1:product:' || sp.producer_no || ':' || p.product_no)::uuid,
       p.product_name || ' («' || sp.brand || '»)',
       'Учебная позиция производства «' || sp.brand || '».',
       round(p.price * (1 + (sp.producer_no - 3) * 0.04), 2),
       p.quantity, p.expiry, p.units_of_measure,
       md5('hierarchy-demo-v1:category:' || p.category_key)::uuid,
       md5('hierarchy-demo-v1:producer:' || sp.producer_no)::uuid,
       NULL
FROM seed_products p
CROSS JOIN seed_producers sp
WHERE true
ON CONFLICT (id) DO NOTHING;

CREATE TEMP TABLE seed_orders (
    order_no integer PRIMARY KEY,
    producer_no integer NOT NULL,
    creation_date timestamptz NOT NULL,
    order_number integer NOT NULL
) ON COMMIT DROP;

-- 600 заказов за 200 дней; 120 заказов на каждое производство.
-- В каждом заказе 2–5 строк, все товары принадлежат одному производству.
INSERT INTO seed_orders (order_no, producer_no, creation_date, order_number)
SELECT n, ((n - 1) % 5) + 1,
       '2026-01-10 08:00:00+03'::timestamptz
           + ((n - 1) / 3) * interval '1 day'
           + ((n - 1) % 3) * interval '3 hour',
       (SELECT COALESCE(MAX(order_number), 0) FROM public.orders) + n
FROM generate_series(1, 600) AS series(n);

INSERT INTO public.orders
    (id, order_number, creation_date, status, manager_id, dispatcher_id, cargo_id,
     production_address, delivery_address, payment_type, requested_delivery_date,
     deadline_confirmation_phase)
SELECT md5('hierarchy-demo-v1:order:' || o.order_no)::uuid,
       o.order_number,
       o.creation_date,
       CASE
           WHEN o.order_no % 10 = 0 THEN 1  -- PendingApproval
           WHEN o.order_no % 10 IN (1, 2, 3, 4) THEN 2  -- Confirmed
           WHEN o.order_no % 10 IN (5, 6, 7) THEN 5  -- AwaitingShipment
           ELSE 7  -- Delivered
       END,
       md5('hierarchy-demo-v1:manager-user')::uuid,
       NULL, NULL,
       sp.address,
       (ARRAY[
           'Москва, ул. Складская, 10',
           'Москва, ул. Промышленная, 24',
           'Химки, ул. Рабочая, 6',
           'Балашиха, ул. Центральная, 18'
       ])[(o.order_no % 4) + 1],
       'Безналичный расчёт',
       o.creation_date + interval '7 days',
       0
FROM seed_orders o
JOIN seed_producers sp ON sp.producer_no = o.producer_no
WHERE true
ON CONFLICT (id) DO NOTHING;

INSERT INTO public.order_lines (id, quantity_of_units, product_id, order_id)
SELECT md5('hierarchy-demo-v1:order-line:' || o.order_no || ':' || line_item.line_no)::uuid,
       2 + ((o.order_no * 3 + line_item.line_no * 7) % 23),
       md5('hierarchy-demo-v1:product:' || o.producer_no || ':' ||
           (((o.order_no * 7 + line_item.line_no * 11) % 40) + 1))::uuid,
       md5('hierarchy-demo-v1:order:' || o.order_no)::uuid
FROM seed_orders o
CROSS JOIN LATERAL generate_series(1, 2 + (o.order_no % 4)) AS line_item(line_no)
WHERE true
ON CONFLICT (id) DO NOTHING;

-- Быстрые проверки внутри транзакции: ошибка откатит всё.
DO $$
DECLARE
    category_count integer;
    company_count integer;
    product_count integer;
    order_count integer;
    line_count integer;
BEGIN
    SELECT count(*) INTO category_count FROM public.categories
    WHERE id IN (SELECT md5('hierarchy-demo-v1:category:' || category_key)::uuid FROM seed_categories);
    SELECT count(*) INTO company_count FROM public.companies
    WHERE id IN (SELECT md5('hierarchy-demo-v1:producer:' || producer_no)::uuid FROM seed_producers);
    SELECT count(*) INTO product_count FROM public.products p
    WHERE p.company_id IN (SELECT md5('hierarchy-demo-v1:producer:' || producer_no)::uuid FROM seed_producers)
      AND p.id IN (
          SELECT md5('hierarchy-demo-v1:product:' || sp.producer_no || ':' || s.product_no)::uuid
          FROM seed_products s CROSS JOIN seed_producers sp
      );
    SELECT count(*) INTO order_count FROM public.orders
    WHERE id IN (SELECT md5('hierarchy-demo-v1:order:' || order_no)::uuid FROM seed_orders);
    SELECT count(*) INTO line_count FROM public.order_lines
    WHERE order_id IN (SELECT md5('hierarchy-demo-v1:order:' || order_no)::uuid FROM seed_orders);

    IF category_count <> 20 OR company_count <> 5 OR product_count <> 200
       OR order_count <> 600 OR line_count <> 2100 THEN
        RAISE EXCEPTION 'Ожидалось 20/5/200/600/2100, получено %/%/%/%/%',
            category_count, company_count, product_count, order_count, line_count;
    END IF;

    IF to_regprocedure('public.refresh_dispatcher_product_rating_daily()') IS NOT NULL THEN
        PERFORM public.refresh_dispatcher_product_rating_daily();
    ELSIF (SELECT relkind FROM pg_class
           WHERE oid = to_regclass('public.dispatcher_product_rating_daily')) = 'm' THEN
        REFRESH MATERIALIZED VIEW public.dispatcher_product_rating_daily;
    END IF;

    RAISE NOTICE 'Готово: % категорий, % производств, % товаров, % заказов, % строк заказов',
        category_count, company_count, product_count, order_count, line_count;
END $$;

COMMIT;
