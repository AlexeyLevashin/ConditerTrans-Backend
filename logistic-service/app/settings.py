import os
from functools import lru_cache
from pathlib import Path
from urllib.parse import quote_plus

from pydantic_settings import BaseSettings, SettingsConfigDict

ROOT_DIR = Path(__file__).resolve().parent.parent


def _resolve_env_file() -> Path:
    env = os.getenv("LOGISTIC_APP_ENV", "local").lower()
    if env == "prod":
        return ROOT_DIR / ".env.prod"
    return ROOT_DIR / ".env.local"


class Settings(BaseSettings):
    model_config = SettingsConfigDict(
        env_file_encoding="utf-8",
        extra="ignore",
    )

    logistic_app_env: str = "local"
    logistic_debug: bool = True

    logistic_db_host: str
    logistic_db_port: int = 5432
    logistic_db_name: str
    logistic_db_user: str
    logistic_db_password: str

    logistic_jwt_issuer: str = "ConditerTrans"
    logistic_jwt_audience: str = "ConditerTrans.Api"
    logistic_jwt_secret_key: str
    logistic_jwt_access_token_expires_minutes: int = 15
    logistic_jwt_refresh_token_expires_days: int = 30

    @property
    def database_url(self) -> str:
        password = quote_plus(self.logistic_db_password)
        return (
            f"postgresql+asyncpg://{self.logistic_db_user}:{password}"
            f"@{self.logistic_db_host}:{self.logistic_db_port}/{self.logistic_db_name}"
        )

    @property
    def alembic_database_url(self) -> str:
        password = quote_plus(self.logistic_db_password)
        return (
            f"postgresql+psycopg://{self.logistic_db_user}:{password}"
            f"@{self.logistic_db_host}:{self.logistic_db_port}/{self.logistic_db_name}"
        )


@lru_cache
def get_settings() -> Settings:
    return Settings(_env_file=_resolve_env_file())
