from sessions.schemas import SessionResponse
from src.sessions.dependencies import get_session_service
from src.sessions.exceptions import SessionNotFound
from src.sessions.service import SessionService
from src.auth.user_role import UserRole
from fastapi import APIRouter, Depends
from src.auth import require_roles
from src.auth.schemas import User
from typing import Annotated

router = APIRouter(
    prefix="/sessions",
    tags=["sessions"]
)


@router.post("/start", response_model=SessionResponse)
async def start_session(
    user: Annotated[User, Depends(require_roles(UserRole.DRIVER))],
    service: Annotated[SessionService, Depends(get_session_service)]
):
    return await service.start_session(user.user_id)


@router.post("/stop")
async def start_session(
    user: Annotated[User, Depends(require_roles(UserRole.DRIVER))],
    service: Annotated[SessionService, Depends(get_session_service)]
):
    await service.stop_session(user.user_id)


@router.get("/active", response_model=SessionResponse)
async def get_active_session(
    user: Annotated[User, Depends(require_roles(UserRole.DRIVER))],
    service: Annotated[SessionService, Depends(get_session_service)]
):
    session = await service.get_active_session_by_user_id(user.user_id)

    if session is None:
        raise SessionNotFound()

    return session
