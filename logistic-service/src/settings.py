from functools import lru_cache
from pathlib import Path
from urllib.parse import quote_plus

from pydantic_settings import BaseSettings, SettingsConfigDict

ROOT_DIR = Path(__file__).resolve().parent.parent


class Settings(BaseSettings):
    service_name: str = "logistic-service"

    model_config = SettingsConfigDict(
        env_file=ROOT_DIR / ".env",
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
        return (
            f"postgresql+asyncpg://{self.logistic_db_user}:{self.logistic_db_password}"
            f"@{self.logistic_db_host}:{self.logistic_db_port}/{self.logistic_db_name}"
        )

    @property
    def alembic_database_url(self) -> str:
        return (
            f"postgresql+psycopg://{self.logistic_db_user}:{self.logistic_db_password}"
            f"@{self.logistic_db_host}:{self.logistic_db_port}/{self.logistic_db_name}"
            f"?options=-c%20search_path={self.service_name}"
        )


@lru_cache
def get_settings() -> Settings:
    return Settings()
