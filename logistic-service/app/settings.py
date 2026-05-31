import os
from functools import lru_cache
from pathlib import Path
from urllib.parse import quote_plus

from pydantic_settings import BaseSettings, SettingsConfigDict

ROOT_DIR = Path(__file__).resolve().parent.parent


def _resolve_env_file() -> Path:
    env = os.getenv("APP_ENV", "local").lower()
    if env == "prod":
        return ROOT_DIR / ".env.prod"
    return ROOT_DIR / ".env.local"


class Settings(BaseSettings):
    model_config = SettingsConfigDict(
        env_file_encoding="utf-8",
        extra="ignore",
    )

    app_env: str = "local"
    debug: bool = True

    db_host: str
    db_port: int = 5432
    db_name: str
    db_user: str
    db_password: str

    jwt_issuer: str = "ConditerTrans"
    jwt_audience: str = "ConditerTrans.Api"
    jwt_secret_key: str
    jwt_access_token_expires_minutes: int = 15
    jwt_refresh_token_expires_days: int = 30

    @property
    def database_url(self) -> str:
        password = quote_plus(self.db_password)
        return (
            f"postgresql+asyncpg://{self.db_user}:{password}"
            f"@{self.db_host}:{self.db_port}/{self.db_name}"
        )

    @property
    def alembic_database_url(self) -> str:
        password = quote_plus(self.db_password)
        return (
            f"postgresql+psycopg://{self.db_user}:{password}"
            f"@{self.db_host}:{self.db_port}/{self.db_name}"
        )


@lru_cache
def get_settings() -> Settings:
    return Settings(_env_file=_resolve_env_file())
