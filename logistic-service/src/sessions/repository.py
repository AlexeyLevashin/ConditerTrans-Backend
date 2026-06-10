from sqlalchemy import insert

from database.repository import BaseRepository
from sessions.models import Session


class SessionRepository(BaseRepository):
    async def create(self, session: Session) -> None:
        await self.session.execute(insert(Session).values({
            "user_id": session.user_id,
            ""
        }))

    async def update(self):
        pass

    async def get_by_user_id(self):
        pass

    async def get_by_id(self):
        pass