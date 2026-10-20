# ConditerTrans Backend

Открывайте в Rider решение `api/ConditerTrans.sln`: оно содержит все семь C#-проектов и ссылки между ними. Для сборки нужен .NET SDK 10.

## Локальный запуск API из Rider

1. Скопируйте `api/API/appsettings.Example.json` в `api/API/appsettings.Development.json`. Задайте собственный `JwtSettings:SecretKey` длиной не меньше 32 символов. Файл Development исключён из Git.
2. Запустите PostgreSQL, Redis и сервис файлов из корня репозитория:

   ```powershell
   docker compose -f docker-compose.yml -f docker-compose.local.yml up -d postgres redis file-service nginx
   ```

   Nginx также запускает контейнер API как свою зависимость. Для API в Rider используется отдельный порт 8080; nginx на порту 80 обслуживает сервис файлов и контейнер API.

3. Выберите профиль `API: http` в Rider. Swagger: `http://localhost:8080/api/swagger`.

В Development используются PostgreSQL `localhost:5432/mydatabase`, Redis `localhost:6379` и сервис файлов через nginx `http://localhost/file-service/`. Для сервиса файлов нужны рабочие настройки S3 в `.env`.

## Полный запуск в Docker

```powershell
docker compose -f docker-compose.yml -f docker-compose.local.yml up --build -d
```

Swagger: `http://localhost/api/swagger`. Внутри Docker адреса сервисов — `postgres`, `redis` и `file-service`; `localhost` внутри контейнера указывает на сам контейнер.

## Проверка сборки

```powershell
dotnet restore api/ConditerTrans.sln
dotnet build api/ConditerTrans.sln --no-restore
```

Если после установки SDK команда `dotnet` не найдена, перезапустите Rider и терминал. Проверить SDK можно командой `& 'C:/Program Files/dotnet/dotnet.exe' --info`; этот же полный путь можно использовать вместо `dotnet` при сборке.

## Витрина рейтинга продукции

`dispatcher_product_rating_daily` — обычная агрегатная таблица. Миграция `20261013000000_ReplaceProductRatingViewWithTable` заменяет прежнее материализованное представление и сразу заполняет таблицу. Миграции выполняются при запуске API.

Гранулярность: одна строка на дату создания заказа в UTC, производственную компанию и название продукта. Ключ — `(company_id, order_date, product_name)`, показатель `order_count` — число различных заказов с этим названием продукта. Несколько строк одного продукта в заказе считаются один раз; разные продукты с одинаковым названием внутри компании объединяются. Заказ с несколькими названиями учитывается в каждом из них, поэтому сумма на графике не равна числу уникальных заказов за день.

Учитываются статусы PendingApproval, Confirmed, Rescheduled, AwaitingShipment, Shipped и Delivered. Черновики, отказы и заказы с продукцией нескольких компаний исключаются.

Hangfire каждые 5 минут вызывает `public.refresh_dispatcher_product_rating_daily()`. Функция полностью пересчитывает таблицу через DELETE и INSERT в одной транзакции: читатели продолжают видеть предыдущие данные до завершения пересчёта. Транзакционная advisory-блокировка сериализует параллельные пересчёты. Новые изменения источников появляются после следующего успешного пересчёта; сама таблица автоматически их не отслеживает.

Ручной пересчёт в DataGrip:

```sql
SELECT public.refresh_dispatcher_product_rating_daily();
```

## Сравнение скорости аналитических запросов

Запросы к обеим витринам и равнозначные выборки из оперативных таблиц находятся в [`scripts/benchmark-data-marts.sql`](scripts/benchmark-data-marts.sql). Порядок измерения и таблица для результатов — в [`scripts/benchmark-data-marts.md`](scripts/benchmark-data-marts.md).
