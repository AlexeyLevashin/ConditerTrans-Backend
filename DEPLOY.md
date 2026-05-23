# Автодеплой (GitHub Actions → VPS)

При пуше в `main` (или `master`) GitHub подключается по SSH и запускает `scripts/deploy.sh`.

## 1. Один раз на сервере

```bash
# репозиторий уже должен быть клонирован
cd /root/ConditerTrans-Backend
git remote -v   # origin → GitHub

chmod +x scripts/deploy.sh scripts/*.sh
```

Убедись, что с сервера работает `git pull` (deploy key или HTTPS token).

## 2. SSH-ключ для GitHub Actions

**На своём ПК** (не на сервере):

```bash
ssh-keygen -t ed25519 -C "github-actions-deploy" -f ./gha_deploy -N ""
```

- `gha_deploy` — приватный ключ → в GitHub Secrets  
- `gha_deploy.pub` — публичный → на сервер  

**На сервере:**

```bash
mkdir -p ~/.ssh
chmod 700 ~/.ssh
nano ~/.ssh/authorized_keys   # вставь содержимое gha_deploy.pub
chmod 600 ~/.ssh/authorized_keys
```

## 3. Secrets в GitHub

Репозиторий → **Settings** → **Secrets and variables** → **Actions** → **New repository secret**

| Secret | Значение |
|--------|----------|
| `SSH_HOST` | `186.246.9.17` |
| `SSH_USER` | `root` |
| `SSH_PRIVATE_KEY` | весь файл `gha_deploy` (с `-----BEGIN...`) |
| `SSH_PORT` | `22` (опционально) |

## 4. Проверка

```bash
git push origin main
```

Вкладка **Actions** → workflow **Deploy to VPS**.

Ручной деплой на сервере:

```bash
/root/ConditerTrans-Backend/scripts/deploy.sh
```

## Безопасность

- Пароль root в чат/репозиторий **не клади** — смени пароль на VPS, если светился.
- Используй только SSH-ключ для деплоя.
