#!/usr/bin/env bash
set -euo pipefail

APP_DIR="${APP_DIR:-/root/ConditerTrans-Backend}"
BRANCH="${BRANCH:-main}"

detect_compose() {
  # v2 стабильнее с новым Docker (v1 падает с KeyError: ContainerConfig)
  if docker compose version >/dev/null 2>&1; then
    DOCKER_COMPOSE=(docker compose)
  elif command -v docker-compose >/dev/null 2>&1; then
    DOCKER_COMPOSE=(docker-compose)
  else
    echo "Нужен docker compose plugin или docker-compose." >&2
    exit 1
  fi
}

compose() {
  "${DOCKER_COMPOSE[@]}" "$@"
}

ensure_scripts_executable() {
  chmod +x "$APP_DIR"/scripts/*.sh 2>/dev/null || true
}

remove_project_containers() {
  local ids
  ids=$(compose ps -aq 2>/dev/null || true)
  if [ -n "$ids" ]; then
    # shellcheck disable=SC2086
    docker rm -f $ids 2>/dev/null || true
  fi
}

detect_compose
COMPOSE_CMD=$(printf '%s' "${DOCKER_COMPOSE[*]}")

cd "$APP_DIR"

echo "### git pull ($BRANCH) ..."
git fetch origin "$BRANCH"
git reset --hard "origin/$BRANCH"
ensure_scripts_executable

echo "### $COMPOSE_CMD down ..."
compose down --remove-orphans 2>/dev/null || true
remove_project_containers

echo "### $COMPOSE_CMD up --build ..."
compose up -d --build

if compose ps nginx 2>/dev/null | grep -q "Up"; then
  echo "### nginx config test & restart ..."
  compose exec -T nginx nginx -t
  compose restart nginx
fi

echo "### done."
compose ps
