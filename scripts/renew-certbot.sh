#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=lib.sh
source "$SCRIPT_DIR/lib.sh"

detect_compose
cd "$(dirname "$SCRIPT_DIR")"

certbot_run renew --quiet
compose exec nginx nginx -s reload

echo "Сертификаты проверены / обновлены."
