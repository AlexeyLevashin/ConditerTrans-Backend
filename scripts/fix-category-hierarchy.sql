-- Разово исправляет дерево, созданное первой версией seed-скрипта.
-- Требует установленную функцию lab_move_category из category-hierarchy-functions.sql.
-- Запускайте весь файл целиком в базе API.

ROLLBACK;
BEGIN;

DO $$
DECLARE
    catalog_root_id uuid;
    branch_key text;
    moved_count integer;
BEGIN
    IF (
        SELECT count(*) FROM public.categories
        WHERE name = 'Кондитерские изделия' AND parent_id IS NULL
    ) <> 1 THEN
        RAISE EXCEPTION 'Нужна ровно одна корневая категория «Кондитерские изделия»';
    END IF;

    SELECT id INTO catalog_root_id
    FROM public.categories
    WHERE name = 'Кондитерские изделия' AND parent_id IS NULL;

    FOREACH branch_key IN ARRAY ARRAY[
        'confectionery', 'bakery', 'desserts', 'sweets'
    ] LOOP
        moved_count := public.lab_move_category(
            md5('hierarchy-demo-v1:category:' || branch_key)::uuid,
            catalog_root_id
        );
        RAISE NOTICE 'Ветка %: обновлено % категорий', branch_key, moved_count;
    END LOOP;
END $$;

COMMIT;

-- После исправления эти четыре категории должны иметь общего родителя.
SELECT child.name AS category,
       parent.name AS parent_category,
       child.path
FROM public.categories child
JOIN public.categories parent ON parent.id = child.parent_id
WHERE child.id IN (
    md5('hierarchy-demo-v1:category:confectionery')::uuid,
    md5('hierarchy-demo-v1:category:bakery')::uuid,
    md5('hierarchy-demo-v1:category:desserts')::uuid,
    md5('hierarchy-demo-v1:category:sweets')::uuid
)
ORDER BY child.name;
