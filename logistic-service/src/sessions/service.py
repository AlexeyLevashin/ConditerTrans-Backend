from src.sessions.exceptions import SessionIsStarted, SessionIsNotStarted
from src.sessions.models import Session
from src.sessions.repository import SessionRepository
from src.sessions.schemas import SessionResponse
from datetime import datetime
from typing import Optional
import uuid


class SessionService:
    def __init__(self, repo: SessionRepository):
        self.repo = repo

    async def start_session(self, user_id: uuid.UUID) -> SessionResponse:
        session = await self.get_active_session_by_user_id(user_id)

        if session is not None:
            raise SessionIsStarted()

        return self.__map_from_database_to_response_model(
            await self.repo.add_one(
                user_id=user_id,
                start_time=datetime.now()
            )
        )

    async def stop_session(self, user_id: uuid.UUID) -> None:
        session = await self.get_active_session_by_user_id(user_id)

        if session is None:
            raise SessionIsNotStarted()

        await self.repo.edit_one_by_id(session.id, end_time=datetime.now())

    async def get_active_session_by_user_id(self, user_id: uuid.UUID) -> Optional[SessionResponse]:
        session = await self.repo.find_one(user_id=user_id, end_time=None)

        if session is None:
            return None

        return self.__map_from_database_to_response_model(session)

    @staticmethod
    def __map_from_database_to_response_model(session: Session) -> SessionResponse:
        return SessionResponse(
            id=session.id,
            user_id=session.user_id,
            start_time=session.start_time,
            end_time=session.end_time,
        )
