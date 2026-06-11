from src.database.repository import SQLAlchemyRepository
from src.sessions.models import Session


class SessionRepository(SQLAlchemyRepository[Session]):
    model = Session
