from sessions.service import SessionService


def get_sessions_service() -> SessionService:
    return SessionService()