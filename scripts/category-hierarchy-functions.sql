-- Четыре функции для лабораторной работы по иерархии категорий.
-- Выполнять после миграции AddCategoryHierarchy. Тестовые данные можно загрузить
-- из scripts/seed-hierarchy-catalog-orders.sql.
-- Категории хранят parent_id и ltree path; имя функции начинается с lab_, чтобы
-- не пересечься с функциями приложения.
-- Запускайте весь файл целиком. Первая команда отменяет незавершённую
-- транзакцию в этой SQL-сессии, если она есть.

ROLLBACK;

-- 1. Все товары в порядке обхода дерева категорий.
-- Путь позволяет видеть уровень и положение категории в иерархии.
CREATE OR REPLACE FUNCTION public.lab_products_in_hierarchy()
RETURNS TABLE (
    category_path ltree,
    category_id uuid,
    category_name text,
    product_id uuid,
    product_name text,
    producer_name text
)
LANGUAGE sql
STABLE
AS $$
    SELECT c.path,
           c.id,
           c.name::text,
           p.id,
           p.name::text,
           co.name::text
    FROM public.categories c
    JOIN public.products p ON p.category_id = c.id
    JOIN public.companies co ON co.id = p.company_id
    ORDER BY c.path, p.name, p.id;
$$;

-- 2. Товары в категориях-потомках категории указанного товара.
-- Товар сам по себе не является узлом дерева. Поэтому «подчинённые товару»
-- трактуются как товары в строго дочерних категориях его категории.
-- Товары в той же категории исключаются.
CREATE OR REPLACE FUNCTION public.lab_products_below_product(p_product_id uuid)
RETURNS TABLE (
    category_path ltree,
    category_id uuid,
    category_name text,
    product_id uuid,
    product_name text,
    producer_name text
)
LANGUAGE sql
STABLE
AS $$
    SELECT child_category.path,
           child_category.id,
           child_category.name::text,
           child_product.id,
           child_product.name::text,
           co.name::text
    FROM public.products source_product
    JOIN public.categories source_category
      ON source_category.id = source_product.category_id
    JOIN public.categories child_category
      ON child_category.path <@ source_category.path -- является потомком или равен
     AND child_category.id <> source_category.id
    JOIN public.products child_product
      ON child_product.category_id = child_category.id
    JOIN public.companies co
      ON co.id = child_product.company_id
    WHERE source_product.id = p_product_id
    ORDER BY child_category.path, child_product.name, child_product.id;
$$;

-- 3. Добавление новой дочерней категории к существующей категории.
-- Возвращает UUID новой категории.
CREATE OR REPLACE FUNCTION public.lab_add_child_category(
    p_parent_id uuid,
    p_name text
)
RETURNS uuid
LANGUAGE plpgsql
AS $$
DECLARE
parent_path ltree;
    new_id uuid;
BEGIN
    IF p_parent_id IS NULL THEN
        RAISE EXCEPTION 'Нужно указать родительскую категорию';
END IF;

    IF p_name IS NULL OR length(btrim(p_name)) = 0 OR length(btrim(p_name)) > 256 THEN
        RAISE EXCEPTION 'Название категории должно содержать от 1 до 256 символов';
END IF;

SELECT c.path INTO parent_path
FROM public.categories c
WHERE c.id = p_parent_id
    FOR UPDATE;

IF NOT FOUND THEN
        RAISE EXCEPTION 'Родительская категория % не найдена', p_parent_id;
END IF;

    new_id := gen_random_uuid();

INSERT INTO public.categories (id, name, parent_id, path)
VALUES (
           new_id,
           btrim(p_name),
           p_parent_id,
           parent_path || text2ltree('c' || replace(new_id::text, '-', ''))
       );

RETURN new_id;
END;
$$;

-- 4. Перенос категории и всех её потомков к новому родителю.
-- Возвращает число категорий, чей path изменился. Если родитель прежний,
-- возвращает 0. Перенос в себя или в собственного потомка запрещён.
CREATE OR REPLACE FUNCTION public.lab_move_category(
    p_category_id uuid,
    p_new_parent_id uuid
)
RETURNS integer
LANGUAGE plpgsql
AS $$
DECLARE
    old_path ltree;
    old_parent_id uuid;
    parent_path ltree;
    new_prefix ltree;
    moved_count integer;
BEGIN
    IF p_category_id IS NULL OR p_new_parent_id IS NULL THEN
        RAISE EXCEPTION 'Нужно указать категорию и нового родителя';
    END IF;

    PERFORM pg_advisory_xact_lock(24001, 1);

    SELECT c.path, c.parent_id INTO old_path, old_parent_id
    FROM public.categories c
    WHERE c.id = p_category_id
    FOR UPDATE;

    IF NOT FOUND THEN
        RAISE EXCEPTION 'Категория % не найдена', p_category_id;
    END IF;

    SELECT c.path INTO parent_path
    FROM public.categories c
    WHERE c.id = p_new_parent_id
    FOR UPDATE;

    IF NOT FOUND THEN
        RAISE EXCEPTION 'Новый родитель % не найден', p_new_parent_id;
    END IF;

    IF parent_path <@ old_path THEN
        RAISE EXCEPTION 'Нельзя переместить категорию в себя или в своего потомка';
    END IF;

    IF old_parent_id IS NOT DISTINCT FROM p_new_parent_id THEN
        RETURN 0;
    END IF;

    new_prefix := parent_path || subpath(old_path, nlevel(old_path) - 1, 1);

    UPDATE public.categories c
    SET path = CASE
                   WHEN c.id = p_category_id THEN new_prefix
                   ELSE new_prefix || subpath(c.path, nlevel(old_path))
               END,
        parent_id = CASE
                        WHEN c.id = p_category_id THEN p_new_parent_id
                        ELSE c.parent_id
                    END
    WHERE c.path <@ old_path;

    GET DIAGNOSTICS moved_count = ROW_COUNT;
    RETURN moved_count;
END;
$$;
