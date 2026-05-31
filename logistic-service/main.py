from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from fastapi.openapi.utils import get_openapi
import uvicorn

from app.routers import tracking
from app.settings import get_settings

settings = get_settings()


# def apply_migrations() -> None:
#     alembic_cfg = Config(str(ROOT_DIR / "alembic.ini"))
#     command.upgrade(alembic_cfg, "head")


# @asynccontextmanager
# async def lifespan(_: FastAPI):
#     try:
#         apply_migrations()
#     except Exception:
#         # Allow local startup even if alembic is misconfigured; tables may already exist.
#         pass
#     yield
#     await close_db()


app = FastAPI(
    title="Logistic Service",
    description="API логистического сервиса",
    version="0.2.0",
    debug=settings.debug,
    docs_url="/swagger",
    redoc_url="/redoc",
    openapi_url="/openapi.json",
)

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=False,
    allow_methods=["*"],
    allow_headers=["*"],
)

app.include_router(tracking.router)


def custom_openapi() -> dict:
    if app.openapi_schema:
        return app.openapi_schema

    schema = get_openapi(
        title=app.title,
        version=app.version,
        description=app.description,
        routes=app.routes,
    )
    schema["info"]["x-environment"] = settings.app_env
    schema.setdefault("components", {}).setdefault("securitySchemes", {})["BearerAuth"] = {
        "type": "http",
        "scheme": "bearer",
        "bearerFormat": "JWT",
    }
    app.openapi_schema = schema
    return app.openapi_schema


app.openapi = custom_openapi


if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=8000)
