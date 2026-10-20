-- Выполните весь файл в SQL-консоли DataGrip для базы API (mydatabase).
-- Требуются применённые миграции API, включая иерархию категорий (ltree).
-- Повторный запуск не добавляет продукты с тем же названием для этой компании.
-- units_of_measure: 0 = штуки, 1 = граммы, 2 = миллилитры.
-- quantity — размер одной товарной единицы, а не остаток на складе.

BEGIN;

DO $$
DECLARE
    target_company_id CONSTANT uuid := '1b0b8bcf-61c9-44c6-9150-f73da30ce21c';
    target_category_id uuid;
    inserted_count integer;
BEGIN
    IF NOT EXISTS (SELECT 1 FROM public.companies WHERE id = target_company_id) THEN
        RAISE EXCEPTION 'Компания % не найдена. Проверьте выбранную базу данных.', target_company_id;
    END IF;

    SELECT id INTO target_category_id
    FROM public.categories
    WHERE name = 'Кондитерские изделия' AND parent_id IS NULL
    ORDER BY id
    LIMIT 1;

    IF target_category_id IS NULL THEN
        target_category_id := gen_random_uuid();
        INSERT INTO public.categories (id, name, parent_id, path)
        VALUES (
            target_category_id,
            'Кондитерские изделия',
            NULL,
            text2ltree('c' || replace(target_category_id::text, '-', ''))
        );
    END IF;

    INSERT INTO public.products
        (id, name, description, price, quantity, expiry, units_of_measure, category_id, company_id, file_id)
    SELECT
        gen_random_uuid(), sample.name, sample.description, sample.price,
        sample.quantity, sample.expiry, sample.units_of_measure,
        target_category_id, target_company_id, NULL
    FROM (VALUES
        ('Торт «Наполеон»', 'Слоёный торт с заварным кремом, упаковка 1000 г.', 1250.00, 1000.0, 3.0, 1),
        ('Торт «Медовик»', 'Медовые коржи со сметанным кремом, упаковка 800 г.', 980.00, 800.0, 5.0, 1),
        ('Печенье овсяное', 'Овсяное печенье, упаковка 300 г.', 180.00, 300.0, 30.0, 1),
        ('Пряники шоколадные', 'Пряники с шоколадной глазурью, упаковка 400 г.', 240.00, 400.0, 45.0, 1),
        ('Зефир ванильный', 'Ванильный зефир, упаковка 250 г.', 210.00, 250.0, 30.0, 1),
        ('Эклер с кремом', 'Поштучный эклер с заварным кремом.', 120.00, 1.0, 2.0, 0)
    ) AS sample(name, description, price, quantity, expiry, units_of_measure)
    WHERE NOT EXISTS (
        SELECT 1 FROM public.products existing
        WHERE existing.company_id = target_company_id AND existing.name = sample.name
    );

    GET DIAGNOSTICS inserted_count = ROW_COUNT;
    RAISE NOTICE 'Добавлено продуктов: %', inserted_count;
END $$;

COMMIT;

SELECT p.id, p.name, p.price, p.quantity, p.expiry, p.units_of_measure,
       c.name AS category
FROM public.products p
JOIN public.categories c ON c.id = p.category_id
WHERE p.company_id = '1b0b8bcf-61c9-44c6-9150-f73da30ce21c'
ORDER BY p.name;
