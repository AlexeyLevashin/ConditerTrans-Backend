# trans-bulki

Expo-приложение (web / iOS / Android) для ConditerTrans.

## Локальная разработка

```bash
cd trans-bulki
npm install
cp .env.example .env.development   # при необходимости
npm run start:local                # или start:web:local
```

Переменные — в `.env.development` / `.env.production`, переключение: `npm run env:local` | `env:prod`.

## Production (Docker)

Статика собирается в образ и отдаётся nginx на порту 80 внутри сети `backend`.

В корневом `.env` можно задать build-args:

```env
EXPO_PUBLIC_API_URL=https://your-domain/api
EXPO_PUBLIC_TRACKING_WS_URL=wss://your-domain/logistic-service
EXPO_PUBLIC_APP_URL=https://your-domain
```

В Nginx Proxy Manager: proxy host на `trans-bulki:80` для `/`, плюс маршруты на `api`, `file-service`, `logistic-service`.
