from typing import Annotated
from fastapi import Depends
from sqlalchemy.ext.asyncio import AsyncSession
from src.database.core.database import get_db
from src.sessions.repository import SessionRepository
from src.sessions.service import SessionService


def get_session_repository(session: Annotated[AsyncSession, Depends(get_db)]):
    return SessionRepository(session)


def get_session_service(repo: Annotated[SessionRepository, Depends(get_session_repository)]) -> SessionService:
    return SessionService(repo)
