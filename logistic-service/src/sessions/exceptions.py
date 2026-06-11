from fastapi import HTTPException
from starlette import status


class SessionIsStarted(HTTPException):
    def __init__(self):
        super().__init__(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail="Смена уже открыта"
        )

class SessionIsNotStarted(HTTPException):
    def __init__(self):
        super().__init__(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail="Смена не была открыта"
        )


class SessionNotFound(HTTPException):
    def __init__(self):
        super().__init__(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Не найдено открытых смен"
        )
