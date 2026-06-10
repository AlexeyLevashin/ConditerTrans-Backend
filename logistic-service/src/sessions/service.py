from sessions.repository import SessionRepository


class SessionService:
    def __init__(self, repo: SessionRepository):
        self.repo = repo

    def start_session(self):
        pass

    def stop_session(self):
        pass
