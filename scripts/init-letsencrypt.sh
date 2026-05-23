#!/usr/bin/env bash
set -euo pipefail

# export CERTBOT_EMAIL="you@example.com"
# export CERTBOT_DOMAIN="conditer-trans.ru"
# export CERTBOT_STAGING=1   # сначала 1 для теста, потом 0 для боевого

CERTBOT_EMAIL="${CERTBOT_EMAIL:-}"
CERTBOT_DOMAIN="${CERTBOT_DOMAIN:-conditer-trans.ru}"
CERTBOT_STAGING="${CERTBOT_STAGING:-0}"

domains=("$CERTBOT_DOMAIN" "www.$CERTBOT_DOMAIN")
rsa_key_size=4096
compose="docker compose --profile certbot"

if [ -z "$CERTBOT_EMAIL" ]; then
  echo "Укажи email: export CERTBOT_EMAIL=\"you@example.com\""
  exit 1
fi

if [ ! -f "docker-compose.yml" ]; then
  echo "Запускай из корня репозитория."
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
$compose run --rm --entrypoint sh certbot -c "
  mkdir -p /etc/letsencrypt/live/$CERTBOT_DOMAIN
  openssl req -x509 -nodes -newkey rsa:$rsa_key_size -days 1 \
    -keyout /etc/letsencrypt/live/$CERTBOT_DOMAIN/privkey.pem \
    -out /etc/letsencrypt/live/$CERTBOT_DOMAIN/fullchain.pem \
    -subj /CN=localhost
"

echo "### Старт nginx + api ..."
docker compose up -d nginx api

echo "### Запрос сертификата Let's Encrypt ..."
domain_args=""
for d in "${domains[@]}"; do
  domain_args="$domain_args -d $d"
done

staging_arg=""
if [ "$CERTBOT_STAGING" = "1" ]; then
  staging_arg="--staging"
fi

$compose run --rm certbot certonly --webroot -w /var/www/certbot \
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
docker compose exec nginx nginx -s reload

echo ""
echo "Готово:"
echo "  https://$CERTBOT_DOMAIN"
echo "  https://$CERTBOT_DOMAIN/swagger"
