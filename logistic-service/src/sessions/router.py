from typing import Annotated
from fastapi import APIRouter, Depends
from src.auth import require_roles
from src.auth.schemas import User
from src.auth.user_role import UserRole

router = APIRouter(
    prefix="/sessions",
    tags=["sessions"]
)


@router.post("/start")
async def start_session(user: Annotated[User, Depends(require_roles(UserRole.DRIVER))]):
    print(user.user_id)
