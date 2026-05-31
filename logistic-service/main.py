from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from fastapi.openapi.utils import get_openapi
from fastapi.responses import JSONResponse
import uvicorn

from app.constants import SERVICE_PREFIX
from app.routers import tracking
from app.settings import get_settings

settings = get_settings()

app = FastAPI(
    title="Logistic Service",
    description="API логистического сервиса",
    version="0.2.0",
    debug=settings.logistic_debug,
    docs_url=f"{SERVICE_PREFIX}/swagger",
    redoc_url=f"{SERVICE_PREFIX}/redoc",
    openapi_url=f"{SERVICE_PREFIX}/openapi.json",
)

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=False,
    allow_methods=["*"],
    allow_headers=["*"],
)


@app.get(f"{SERVICE_PREFIX}/health", tags=["service"])
async def health() -> JSONResponse:
    return JSONResponse({"status": "ok", "service": "logistic-service"})


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
    schema["info"]["x-environment"] = settings.logistic_app_env
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
