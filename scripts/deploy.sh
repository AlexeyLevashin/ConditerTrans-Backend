#!/usr/bin/env bash
set -euo pipefail

APP_DIR="${APP_DIR:-/root/ConditerTrans-Backend}"
BRANCH="${BRANCH:-main}"

detect_compose() {
  # На VPS часто только docker-compose (v1), а "docker compose" ломается
  if command -v docker-compose >/dev/null 2>&1; then
    DOCKER_COMPOSE=(docker-compose)
  elif docker compose version >/dev/null 2>&1; then
    DOCKER_COMPOSE=(docker compose)
  else
    echo "Нужен docker-compose или docker compose plugin." >&2
    exit 1
  fi
}

compose() {
  "${DOCKER_COMPOSE[@]}" "$@"
}

ensure_scripts_executable() {
  chmod +x "$APP_DIR"/scripts/*.sh 2>/dev/null || true
}

detect_compose
COMPOSE_CMD=$(printf '%s' "${DOCKER_COMPOSE[*]}")

cd "$APP_DIR"

echo "### git pull ($BRANCH) ..."
git fetch origin "$BRANCH"
git reset --hard "origin/$BRANCH"
ensure_scripts_executable

echo "### $COMPOSE_CMD up --build ..."
compose up -d --build

if compose ps nginx 2>/dev/null | grep -q "Up"; then
  echo "### nginx reload ..."
  compose exec -T nginx nginx -s reload
fi

echo "### done."
compose ps
