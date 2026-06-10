from fastapi.middleware.cors import CORSMiddleware
from src.settings import get_settings
from src.routers import routers
from fastapi import FastAPI
import uvicorn

settings = get_settings()

app = FastAPI(
    title="Logistic Service",
    description="API логистического сервиса",
    version="0.2.0",
    debug=settings.logistic_debug
)

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=False,
    allow_methods=["*"],
    allow_headers=["*"],
)


for router in routers:
    app.include_router(router)

if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=8000)
