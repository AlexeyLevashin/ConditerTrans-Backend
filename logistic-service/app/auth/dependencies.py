from collections.abc import Callable
from typing import Annotated

from fastapi import Depends, HTTPException, status
from fastapi.security import HTTPAuthorizationCredentials, HTTPBearer

from app.auth.jwt import JwtValidationError, decode_access_token
from app.enums.user_role import UserRole
from app.schemas.auth import CurrentUser
from app.settings import Settings, get_settings

bearer_scheme = HTTPBearer(auto_error=False)


def _forbidden() -> HTTPException:
    return HTTPException(status_code=status.HTTP_403_FORBIDDEN, detail="Forbidden")


async def get_current_user(
    credentials: Annotated[HTTPAuthorizationCredentials | None, Depends(bearer_scheme)],
    settings: Annotated[Settings, Depends(get_settings)],
) -> CurrentUser:
    if credentials is None or credentials.scheme.lower() != "bearer":
        raise _forbidden()

    try:
        return decode_access_token(credentials.credentials, settings)
    except JwtValidationError:
        raise _forbidden() from None


def require_roles(*roles: UserRole) -> Callable[..., CurrentUser]:
    async def checker(
        current_user: Annotated[CurrentUser, Depends(get_current_user)],
    ) -> CurrentUser:
        if not current_user.has_any_role(*roles):
            raise _forbidden()
        return current_user

    return checker
