# ConditerTrans-Backend — документация для ИИ-агентов

> **Назначение:** единый контекст для AI-агентов, работающих с монорепозиторием ConditerTrans (КондитерТранс / ТрансЛогистик). Читай этот файл **до** изменений в коде.

## Кратко о продукте

B2B-платформа логистики кондитерских заказов между компаниями.

| Участник | Роль в системе | Что делает |
|----------|----------------|------------|
| Менеджер закупок | `Manager` | Формирует заказ из каталога продуктов, отправляет на согласование |
| Диспетчер производства | `Dispatcher` | Подтверждает/отклоняет/переносит заказы, готовит к отгрузке |
| Координатор логистики | `Coordinator` | Назначает водителя и транспорт на груз |
| Водитель | `Driver` | Везёт груз, публикует GPS (tracking в logistic-service **не реализован**) |

**Сервисы:** `api` (.NET 10), `ConditerTrans-Frontend` (Expo 54), `file-service`, `logistic-service` (Python/FastAPI).

---

## Структура монорепозитория

```
ConditerTrans-Backend/
├── api/                      # ASP.NET Core 10 — главный REST API
├── ConditerTrans-Frontend/   # Expo 54 — web/iOS/Android UI (отдельный git-репо внутри)
├── logistic-service/         # FastAPI — сессии водителей, WS-уведомления
├── file-service/             # FastAPI — загрузка файлов в S3
├── nginx/                    # Справочные конфиги маршрутизации (не в compose)
├── docker-compose.yml        # Backend-оркестрация (фронт **не** в compose)
├── docker-compose.local.yml
├── scripts/deploy.sh
├── .github/workflows/
└── AGENTS.md
```

**Фронтенд:** каталог `ConditerTrans-Frontend/` — clone [CandyOrganization/ConditerTrans-Frontend](https://github.com/CandyOrganization/ConditerTrans-Frontend). Содержит собственный `.git`. В `package.json` legacy-имя `trans-bulki` (slug Expo).

**Не трогай без необходимости:** `npm/`, `certbot/`, `is-is-otchet5.docx`, `scripts/update_otchet5_docx.py`.

> **История:** старый каталог `trans-bulki/` удалён. Актуальный фронт — только `ConditerTrans-Frontend/`.

---

## Архитектура

```
                    ┌─────────────────────┐
  Browser / App ──► │ Nginx Proxy Manager │ :80/:443
                    └─────────┬───────────┘
          ┌───────────────────┼───────────────────┐
          ▼                   ▼                   ▼
   (frontend SPA)        api:8080          file-service:80
   Expo web build              │           logistic-service:8000
                               ▼
                        PostgreSQL (внешний)
                               ▲
                        S3 (Timeweb)
```

| Маршрут (через NPM) | Сервис | Назначение |
|---------------------|--------|------------|
| `/` | frontend (static) | SPA «ТрансЛогистик» — **настраивается в NPM вручную** |
| `/api/` | api | REST API, Swagger `/api/swagger` |
| `/file-service/` | file-service | Файлы продуктов |
| `/logistic-service/` | logistic-service | Сессии, WS (GPS tracking — не реализован) |

**Docker compose** поднимает только backend: `api`, `file-service`, `logistic-service`, `nginx-proxy-manager`. Фронт запускается локально через Expo или деплоится отдельно.

---

## Доменная модель

### Роли (`Common.Enums.UserRole`)

`Manager` · `Dispatcher` · `Coordinator` · `Driver`

Флаг `isAdmin` — приглашение сотрудников (`POST /api/users/admin-invite`).

### Типы компаний (`CompanyType`)

- `PurchasingCompany` — закупающая (менеджеры)
- `ProductionDispatcher` — производство (диспетчеры)
- `LogisticCompany` — логистика (координаторы, водители)

### Жизненный цикл заказа (`OrderStatus`)

```
Draft → PendingApproval → Confirmed | Rescheduled | Rejected
                              ↓
                    AwaitingShipment → Shipped → Delivered
```

- **Draft** — черновик менеджера (один на пользователя)
- **PendingApproval** — отправлен диспетчеру
- **Rescheduled** — диспетчер предложил новую дату; менеджер accept/reject
- **AwaitingShipment** — готов к забору логистикой
- **Shipped** — передан логистике (создаётся `Cargo`)
- Hangfire: автоподтверждение дедлайна за 2 дня до доставки (`OrderDeadlineConfirmationService`)

### Жизненный цикл груза (`CargoStatus`)

```
NotAssignedToLogisticCompany → AwaitingTransportation → PickedUpFromProduction → Delivered
                                                                              ↘ Cancelled
```

### Связи сущностей (упрощённо)

```
Company ──< Employee ── User
Company ──< Product >── Category
User(Manager) ──< Order >── OrderLine >── Product
Order ── (0..1) Cargo ── Driver, TransportVehicle, LogisticCompany
Order ──< OrderChangeHistory
Cargo ──< CargoChangeHistory
```

---

## Сервис: `api/` (.NET 10)

### Слои (Clean Architecture)

| Проект | Содержимое |
|--------|------------|
| **API** | Controllers, middleware, Hangfire, Swagger |
| **Application** | Services, интерфейсы, mappers, validators |
| **Infrastructure** | Repositories, JWT, BCrypt, HTTP-клиент file-service |
| **DataAccess** | EF Core, migrations, configurations |
| **Domain** | Entities |
| **Contracts** | Request/Response DTO |
| **Common** | Enums, helpers |

**DI:** `Program.cs` → `AddDataAccess` → `AddApplication` → `AddInfrastructure` → `AddOrderDeadlineHangfire`.

### Контроллеры и маршруты

Все маршруты с префиксом `api/`. Базовый `BaseController` — `[Authorize]`. **Auth — публичный.**

| Контроллер | Route | Ключевые операции |
|------------|-------|-------------------|
| AuthController | `api/auth` | login, refresh, set-password, create admin |
| UserController | `api/users` | me, employees, drivers, invite, change-password |
| CompanyController | `api/companies` | list, manager/production |
| ProductController | `api/products` | get, list (paged, filters) |
| CategoryController | `api/categories` | list |
| OrderController | `api/orders` | manager + dispatcher flows, reports |
| CargoController | `api/cargo` | coordinator/driver views, assign-driver |
| TransportVehicleController | `api/transport-vehicles` | brands, models, available, create |
| ReportController | `api/reports` | coordinator/free-transport |

Подробнее: `ConditerTrans-Frontend/docs/dispatcher-api-plan.md`, `manager-reschedule-api.md`, `dispatcher-reports.md`.

### API диспетчера (`OrderController` / `OrderService`)

Диспетчер видит заказы, где все позиции от его производства (`Product.CompanyId == CompanyId` из JWT). Черновики (`Draft`) скрыты.

| Method | Path | Действие |
|--------|------|----------|
| GET | `/api/orders/dispatcher` | Список (`search`, `status`, paging) |
| GET | `/api/orders/dispatcher/{id}` | Детали + строки |
| POST | `/api/orders/dispatcher/{id}/confirm` | → `Confirmed` |
| POST | `/api/orders/dispatcher/{id}/reject` | `{ "reason" }` → `Rejected` |
| POST | `/api/orders/dispatcher/{id}/reschedule` | `{ "newDeliveryDate", "reason" }` → `Rescheduled` |
| POST | `/api/orders/dispatcher/{id}/ready-for-shipment` | `{ "shipmentDate" }` → `AwaitingShipment` |
| POST | `/api/orders/dispatcher/{id}/handover` | `{ "documentsHandedOver" }` → `Shipped` |

Менеджер пересогласовывает перенос: `POST /api/orders/{id}/reschedule/accept|reject`.

### Паттерны кода

1. **Авторизация** — в сервисах, не через `[Authorize(Roles=...)]`. Ошибки — русские строки → `Forbid()`.
2. **Результаты** — `FluentResults` (`Result` / `Result<T>`).
3. **Маппинг** — статические `*Mapper` + Mapster.
4. **Repository + UnitOfWork** — интерфейсы в Application, реализации в Infrastructure.
5. **ID** — `Guid.CreateVersion7()`.
6. **БД** — snake_case в PostgreSQL; конфиги в `DataAccess/Configurations/`.
7. **JWT claims** — `sub`, `email`, `role`, `CompanyId`.
8. **Миграции** — EF Core, применяются при старте.

### Локальный запуск

```bash
cd api
dotnet ef database update --project DataAccess --startup-project API
dotnet run --project API
```

Swagger: `http://localhost:8080/api/swagger`

---

## Сервис: `ConditerTrans-Frontend/` (Expo 54)

### Стек

React Native 0.81 · React 19 · Expo Router 6 · TypeScript · AsyncStorage · Leaflet (карты) · WebSocket (logistic-service).

### Структура

```
ConditerTrans-Frontend/
├── app/                    # Expo Router — экраны
├── src/
│   ├── api/                # HTTP-клиенты по доменам
│   ├── context/AuthContext.tsx
│   ├── components/         # UI по фичам (Manager/, Dispatcher/, Modal/, …)
│   ├── hooks/              # GPS / WS подписки
│   ├── services/           # cargoTrackingWebSocket
│   ├── config/env.ts
│   └── types/
├── docs/                   # API- и тестовая документация, SQL seeds
└── scripts/use-env.mjs
```

Alias: `@/*` → `./src/*`

### Экраны (`app/`)

| Path | Назначение |
|------|------------|
| `/` | Dashboard по роли |
| `/login`, `/set-password` | Auth, приглашения |
| `/profile`, `/employees` | Профиль, приглашение сотрудников (admin) |
| `/reports` | Отчёты Coordinator / Dispatcher |
| `/order/[orderId]` | Детали заказа (Dispatcher) |
| `/cargo/[cargoId]` | Карта груза + GPS (Coordinator/Driver) |
| `/trip/[tripId]` | Legacy mock |

### Роли → UI и API

| Роль | Dashboard | Backend |
|------|-----------|---------|
| **Coordinator** | Pending cargos (paginated), active trips, assign driver | ✅ `cargo.ts`, `applications.ts` |
| **Driver** | Active cargos, GPS publish | ✅ (tracking WS — см. пробелы) |
| **Dispatcher** | `DispatcherOrdersPanel`, order modals | ✅ `dispatcherOrders.ts` |
| **Dispatcher** | Reports (refusals, product rating) | ✅ `dispatcherReports.ts` |
| **Manager** | `ManagerOrderHistoryPanel` (history, repeat) | ✅ `managerOrders.ts` |
| **Admin** | Employees invite | ✅ `users.ts` |

Legacy mock: `trips.ts`, `tripDetails.ts`, часть `reports.ts` (coordinator free transport).

### API-модули (`src/api/`)

| Модуль | Backend |
|--------|---------|
| `client.ts`, `tokenStorage.ts` | HTTP + AsyncStorage auth |
| `auth.ts`, `users.ts`, `profile.ts` | `/auth/*`, `/users/*` |
| `cargo.ts`, `applications.ts` | `/cargo/*` |
| `dispatcherOrders.ts`, `dispatcherReports.ts` | `/orders/dispatcher/*` |
| `managerOrders.ts`, `managerReports.ts` | `/orders/*` (manager flows) |
| `products.ts`, `companies.ts`, `categories` via products | каталог |
| `transportVehicles.ts`, `drivers.ts` | транспорт, водители |
| `paymentMethod.ts` | способы оплаты |

Центральный клиент: `apiRequest<T>()` — Bearer token, `ApiError`, парсинг ProblemDetails.

### Auth

- Storage: AsyncStorage key `translogistic_auth` (`tokenStorage.ts`)
- Login / set-password → tokens → enrich via `GET /users/me`
- **Refresh token сохраняется, ротация не реализована** (`POST /auth/refresh` не вызывается)
- Auth gate per-screen: `<Redirect href="/login" />`

### Env (`EXPO_PUBLIC_*`)

| Переменная | По умолчанию |
|------------|--------------|
| `EXPO_PUBLIC_API_URL` | `http://localhost:8080/api` |
| `EXPO_PUBLIC_TRACKING_WS_URL` | `ws://localhost/logistic-service` |
| `EXPO_PUBLIC_APP_URL` | invite links base |

Переключение: `npm run env:local` / `env:prod`. Запуск: `npm run start:local`, `web:local`, и т.д.

### Локальная разработка

```bash
cd ConditerTrans-Frontend
npm install
cp .env.example .env.development   # при необходимости
npm run start:local                # или web:local
```

### Паттерны фронтенда

- State: React Context (`AuthContext`) + локальный `useState`/`useEffect`
- Pagination: `DispatcherPagination` (coordinator, dispatcher, manager)
- Карты: `CargoMap.web.tsx` (react-leaflet) vs `CargoMap.tsx` (WebView)
- UI-kit: `src/components/ui/Ui.tsx`, палитра `src/theme/colors.ts`
- Язык UI — **русский**

---

## Сервис: `logistic-service/` (Python/FastAPI)

**Порт:** 8000 · **Entry:** `main.py`

| Method | Path | Auth | Описание |
|--------|------|------|----------|
| POST | `/sessions/start` | Driver JWT | Начать смену |
| POST | `/sessions/stop` | Driver JWT | Завершить смену |
| GET | `/sessions/active` | Driver JWT | Активная сессия |
| POST | `/orders/notify` | **нет** | Broadcast WS-клиентам |
| WS | `/ws/orders/new` | Coordinator JWT | Уведомления о заказах |

### Не реализовано (ожидает фронт)

- `WS /tracking/ws/cargo/{cargoId}?role=subscriber|driver`
- `GET /tracking/cargo/{cargoId}/history`
- `coordinates/router.py` — **пустой**

JWT: issuer/audience/secret совпадают с `api`. БД: schema `logistic-service`, таблица `sessions`.

---

## Сервис: `file-service/` (Python/FastAPI)

**Порт:** 80

| Method | Path | Описание |
|--------|------|----------|
| POST | `/file-service/file` | Upload |
| GET | `/file-service/file/{id}` | Metadata + S3 URL |
| GET | `/file-service/files?file_ids=...` | Batch lookup |

Main API → `FileServiceClient`. Storage: S3 (Timeweb).

---

## Бизнес-потоки (куда смотреть)

| Поток | Backend | Frontend |
|-------|---------|----------|
| Менеджер: draft → submit | `OrderService`, `OrderController` | `managerOrders.ts`, `ManagerOrderHistoryPanel` |
| Диспетчер: confirm/reject/… | `OrderService` | `dispatcherOrders.ts`, `/order/[orderId]` |
| Логистика: assign driver | `CargoService`, `CargoController` | `ProcessApplicationModal`, `cargo.ts` |
| Дедлайны | Hangfire `OrderDeadlineConfirmationService` | — |

---

## Разработка и деплой

### Docker (backend)

```bash
docker compose up --build
# или с local overrides:
docker compose -f docker-compose.yml -f docker-compose.local.yml up --build
```

Сервисы: `api`, `file-service`, `logistic-service`, `nginx-proxy-manager`.

### Деплой

Push `main`/`master` → GitHub Actions → `scripts/deploy.sh`.

NPM на VPS: proxy `/api`, `/file-service`, `/logistic-service` (см. `nginx/conf.d/`), плюс frontend на `/`.

### Миграции

```bash
dotnet ef migrations add Name --project api/DataAccess --startup-project api/API
dotnet ef database update --project api/DataAccess --startup-project api/API
cd logistic-service && alembic upgrade head
```

---

## Известные пробелы и технический долг

| Область | Статус |
|---------|--------|
| GPS tracking | Фронт готов (WS hooks), бэкенд не реализован |
| Refresh token (frontend) | Endpoint есть, клиент не вызывает |
| Manager: полный flow создания заказа | API есть; UI — history/repeat, без полноценного конструктора |
| Coordinator free transport report | Mock в `reports.ts` |
| Legacy `/trip/[tripId]` | Mock |
| Frontend в docker-compose | **Не подключён** |
| `IEmployeeService` | Не в DI |
| `ICompanyService` | Дублируется в DI |
| Тесты | **Отсутствуют** |
| Hangfire dashboard | Без auth |
| CORS API | `AllowAll` |
| `POST /orders/notify` | Без auth |
| `.gitignore` | Всё ещё ссылается на `trans-bulki/` — legacy |

---

## Правила для ИИ-агентов

### Перед изменениями

1. Определи сервис: `api` / `ConditerTrans-Frontend` / `logistic-service` / `file-service`.
2. Проверь роль и статус сущности — логика role-aware.
3. При изменении API — обнови модуль в `ConditerTrans-Frontend/src/api/` и типы в `src/types/`.
4. DTO → `Contracts/` + маппер; snake_case только в БД.
5. Документация по API-контрактам — в `ConditerTrans-Frontend/docs/`.

### Стиль

- Минимальный diff
- Ошибки пользователю — **на русском**
- Не коммить секреты
- Не создавай markdown без запроса (кроме AGENTS.md)
- Фронтенд — **`ConditerTrans-Frontend/`**, не `trans-bulki/`

### Типичные задачи → файлы

| Задача | Где |
|--------|-----|
| REST endpoint | `api/API/Controllers/`, `Application/*Service.cs`, `Contracts/` |
| Frontend screen | `ConditerTrans-Frontend/app/` |
| Frontend API call | `ConditerTrans-Frontend/src/api/<domain>.ts` |
| Real-time / GPS | `logistic-service/src/coordinates/`, `ConditerTrans-Frontend/src/hooks/` |
| SQL seeds / schema docs | `ConditerTrans-Frontend/docs/database/` |

### Верификация

```bash
dotnet build api/ConditerTrans.sln
cd ConditerTrans-Frontend && npx tsc --noEmit
cd logistic-service && python -m compileall .
cd file-service && python -m compileall .
```

Swagger: `/api/swagger`. Автотестов нет.

---

## Env-переменные

| Scope | Переменные |
|-------|------------|
| Корневой `.env` | `DB_*`, `S3_*`, `LOGISTIC_*` |
| API | `ASPNETCORE_*`, `ConnectionStrings__DefaultConnection` |
| Frontend | `EXPO_PUBLIC_API_URL`, `EXPO_PUBLIC_TRACKING_WS_URL`, `EXPO_PUBLIC_APP_URL` |

---

## Полезные ссылки

| Файл | Содержание |
|------|------------|
| `api/ConditerTrans.sln` | .NET solution |
| `api/Application/Orders/OrderService.cs` | Заказы |
| `ConditerTrans-Frontend/app/index.tsx` | Role dashboard |
| `ConditerTrans-Frontend/src/api/client.ts` | HTTP client |
| `ConditerTrans-Frontend/docs/dispatcher-api-plan.md` | API диспетчера |
| `ConditerTrans-Frontend/docs/manager-reschedule-api.md` | Перенос сроков |
| `ConditerTrans-Frontend/docs/api-pagination.md` | Пагинация |
| `docker-compose.yml` | Backend compose |
| `nginx/conf.d/02-https.conf` | HTTPS-маршруты |

---

*Последнее обновление: 2026-06-13.*
