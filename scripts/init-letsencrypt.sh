#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=lib.sh
source "$SCRIPT_DIR/lib.sh"

# export CERTBOT_EMAIL="you@example.com"
# export CERTBOT_DOMAIN="conditer-trans.ru"
# export CERTBOT_STAGING=1   # сначала 1 для теста, потом 0 для боевого

CERTBOT_EMAIL="${CERTBOT_EMAIL:-}"
CERTBOT_DOMAIN="${CERTBOT_DOMAIN:-conditer-trans.ru}"
CERTBOT_STAGING="${CERTBOT_STAGING:-0}"

domains=("$CERTBOT_DOMAIN" "www.$CERTBOT_DOMAIN")
rsa_key_size=4096

detect_compose

if [ -z "$CERTBOT_EMAIL" ]; then
  echo "Укажи email: export CERTBOT_EMAIL=\"you@example.com\""
  exit 1
fi

cd "$(dirname "$SCRIPT_DIR")"

if [ ! -f "docker-compose.yml" ]; then
  echo "docker-compose.yml не найден в $(pwd)"
  exit 1
fi

mkdir -p certbot/www certbot/conf

if [ -d "certbot/conf/live/$CERTBOT_DOMAIN" ]; then
  read -r -p "Сертификат уже есть. Перевыпустить? (y/N) " decision
  if [ "$decision" != "y" ] && [ "$decision" != "Y" ]; then
    exit 0
  fi
fi

echo "### Временный сертификат (nginx стартует с HTTPS-конфигом) ..."
certbot_sh "
  mkdir -p /etc/letsencrypt/live/$CERTBOT_DOMAIN
  openssl req -x509 -nodes -newkey rsa:$rsa_key_size -days 1 \
    -keyout /etc/letsencrypt/live/$CERTBOT_DOMAIN/privkey.pem \
    -out /etc/letsencrypt/live/$CERTBOT_DOMAIN/fullchain.pem \
    -subj /CN=localhost
"

echo "### Старт nginx + api ..."
compose up -d nginx api

echo "### Запрос сертификата Let's Encrypt ..."
domain_args=""
for d in "${domains[@]}"; do
  domain_args="$domain_args -d $d"
done

staging_arg=""
if [ "$CERTBOT_STAGING" = "1" ]; then
  staging_arg="--staging"
fi

# shellcheck disable=SC2086
certbot_run certonly --webroot -w /var/www/certbot \
  $staging_arg \
  $domain_args \
  --email "$CERTBOT_EMAIL" \
  --rsa-key-size "$rsa_key_size" \
  --agree-tos \
  --no-eff-email \
  --force-renewal

echo "### HTTP -> HTTPS редирект ..."
cp nginx/conf.d/01-http.after-ssl.conf nginx/conf.d/01-http.conf

echo "### Перезагрузка nginx ..."
compose exec nginx nginx -s reload

echo ""
echo "Готово:"
echo "  https://$CERTBOT_DOMAIN"
echo "  https://$CERTBOT_DOMAIN/swagger"
