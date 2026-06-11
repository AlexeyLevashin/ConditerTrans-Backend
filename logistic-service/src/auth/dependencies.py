from collections.abc import Callable
from typing import Annotated
from fastapi import Depends, WebSocket, WebSocketException, status
from fastapi.security import HTTPAuthorizationCredentials, HTTPBearer
from src.auth.exceptions import AuthForbidden
from src.auth.jwt import JwtValidationError, decode_access_token
from src.auth.user_role import UserRole
from src.auth.schemas import User
from src.settings import Settings, get_settings

bearer_scheme = HTTPBearer(auto_error=False)


async def get_current_user(
    credentials: Annotated[HTTPAuthorizationCredentials | None, Depends(bearer_scheme)],
    settings: Annotated[Settings, Depends(get_settings)],
) -> User:
    if credentials is None or credentials.scheme.lower() != "bearer":
        raise AuthForbidden()
    try:
        return decode_access_token(credentials.credentials, settings)
    except JwtValidationError:
        raise AuthForbidden()


def require_roles(*roles: UserRole) -> Callable[..., User]:
    def checker(
        current_user: Annotated[User, Depends(get_current_user)],
    ) -> User:
        if not current_user.has_any_role(*roles):
            raise AuthForbidden()
        return current_user

    return checker


def _extract_ws_token(websocket: WebSocket) -> str | None:
    auth = websocket.headers.get("authorization")
    if auth and auth.lower().startswith("bearer "):
        return auth[7:].strip()

    return None


async def get_current_user_ws(
    websocket: WebSocket,
    settings: Annotated[Settings, Depends(get_settings)],
) -> User:
    token = _extract_ws_token(websocket)
    if not token:
        raise WebSocketException(code=status.WS_1008_POLICY_VIOLATION)
    try:
        return decode_access_token(token, settings)
    except JwtValidationError:
        raise WebSocketException(code=status.WS_1008_POLICY_VIOLATION)


def require_roles_ws(*roles: UserRole) -> Callable[..., User]:
    def checker(
        current_user: Annotated[User, Depends(get_current_user_ws)],
    ) -> User:
        if not current_user.has_any_role(*roles):
            raise WebSocketException(code=status.WS_1008_POLICY_VIOLATION)
        return current_user

    return checker
