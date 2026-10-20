-- Демонстрация четырёх функций из category-hierarchy-functions.sql.
-- Требуются применённые миграции, загруженный seed-hierarchy-catalog-orders.sql
-- и выполненный category-hierarchy-functions.sql.
-- Запускайте весь файл в одной SQL-сессии. Первый ROLLBACK очищает ошибочное
-- состояние предыдущей транзакции, если оно осталось в консоли.

ROLLBACK;

-- 1. Товары в порядке дерева категорий.
SELECT item.category_name,
       nlevel(item.category_path) AS category_level,
       item.product_name,
       item.producer_name
FROM public.lab_products_in_hierarchy() item
ORDER BY item.category_path, item.product_name, item.product_id
LIMIT 30;

-- 2. Товары в дочерних категориях категории товара «Пряники заварные».
-- У этого товара категория «Пряники»; в seed есть дочерняя категория
-- «Пряники глазированные», поэтому результат должен быть непустым.
SELECT * FROM public.lab_products_below_product(
    md5('hierarchy-demo-v1:product:1:1')::uuid
)
ORDER BY category_path, product_name, product_id;

-- 3–4. Создание категории и перенос под другого родителя.
-- Изменения откатываются в конце, чтобы пример можно было запускать повторно.
BEGIN;

DROP TABLE IF EXISTS pg_temp.demo_new_category;
CREATE TEMP TABLE demo_new_category (id uuid PRIMARY KEY) ON COMMIT DROP;

INSERT INTO demo_new_category (id)
SELECT public.lab_add_child_category(
    md5('hierarchy-demo-v1:category:gingerbread')::uuid,
    'Пряники с ореховой начинкой'
);

SELECT c.id, c.name, c.parent_id, c.path
FROM public.categories c
JOIN demo_new_category d ON d.id = c.id;

SELECT public.lab_add_child_category(
    (SELECT id FROM demo_new_category),
    'Пряники с фундуком'
) AS nested_category_id;

SELECT public.lab_move_category(
    (SELECT id FROM demo_new_category),
    md5('hierarchy-demo-v1:category:waffles')::uuid
) AS moved_categories;

SELECT c.id, c.name, parent.name AS parent_name, c.path
FROM public.categories c
JOIN demo_new_category d ON c.path <@ (SELECT path FROM public.categories WHERE id = d.id)
JOIN public.categories parent ON parent.id = c.parent_id;

ROLLBACK;
