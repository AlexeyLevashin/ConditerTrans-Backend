#!/usr/bin/env bash

detect_compose() {
  if docker compose version >/dev/null 2>&1; then
    DOCKER_COMPOSE=(docker compose)
  elif command -v docker-compose >/dev/null 2>&1; then
    DOCKER_COMPOSE=(docker-compose)
  else
    echo "Нужен Docker Compose: установи плагин 'docker compose' или пакет docker-compose." >&2
    exit 1
  fi
}

compose() {
  "${DOCKER_COMPOSE[@]}" "$@"
}

certbot_run() {
  docker run --rm \
    -v "$(pwd)/certbot/www:/var/www/certbot" \
    -v "$(pwd)/certbot/conf:/etc/letsencrypt" \
    certbot/certbot:latest "$@"
}

certbot_sh() {
  docker run --rm \
    -v "$(pwd)/certbot/www:/var/www/certbot" \
    -v "$(pwd)/certbot/conf:/etc/letsencrypt" \
    --entrypoint sh \
    certbot/certbot:latest -c "$1"
}
