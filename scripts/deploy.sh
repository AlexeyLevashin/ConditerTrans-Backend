#!/usr/bin/env bash
set -euo pipefail

APP_DIR="${APP_DIR:-/root/ConditerTrans-Backend}"
BRANCH="${BRANCH:-main}"

cd "$APP_DIR"

echo "### git pull ($BRANCH) ..."
git fetch origin "$BRANCH"
git reset --hard "origin/$BRANCH"

echo "### docker compose up --build ..."
docker compose up -d --build

if docker compose ps --status running nginx >/dev/null 2>&1; then
  echo "### nginx reload ..."
  docker compose exec -T nginx nginx -s reload
fi

echo "### done."
docker compose ps
