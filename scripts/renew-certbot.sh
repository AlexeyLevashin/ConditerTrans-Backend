#!/usr/bin/env bash
set -euo pipefail

docker compose --profile certbot run --rm certbot renew --quiet
docker compose exec nginx nginx -s reload

echo "Сертификаты проверены / обновлены."
