from typing import Any
import jwt
from jwt.exceptions import InvalidTokenError
from src.auth.schemas import User
from src.auth.user_role import UserRole
from src.settings import Settings

ROLE_CLAIM_KEYS = (
    "role",
    "roles",
    "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
)
USER_ID_CLAIM_KEYS = ("sub", "nameid", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")


class JwtValidationError(Exception):
    pass


def _extract_claim(payload: dict[str, Any], keys: tuple[str, ...]) -> str | None:
    for key in keys:
        value = payload.get(key)
        if value is not None and str(value).strip():
            return str(value)
    return None


def _parse_role(payload: dict[str, Any]) -> UserRole:
    raw_role: str | None = None

    for key in ROLE_CLAIM_KEYS:
        value = payload.get(key)
        if value is None:
            continue
        if isinstance(value, str):
            raw_role = value
            break
        if isinstance(value, list):
            if len(value) != 1:
                raise JwtValidationError("User must have exactly one role")
            raw_role = str(value[0])
            break

    if not raw_role:
        raise JwtValidationError("Role claim is missing")

    try:
        return UserRole(raw_role)
    except ValueError as exc:
        raise JwtValidationError(f"Unknown role: {raw_role}") from exc


def decode_access_token(token: str, settings: Settings) -> User:
    try:
        payload = jwt.decode(
            token,
            settings.logistic_jwt_secret_key,
            algorithms=["HS256"],
            audience=settings.logistic_jwt_audience,
            issuer=settings.logistic_jwt_issuer,
        )
    except InvalidTokenError as exc:
        raise JwtValidationError("Invalid token") from exc

    user_id = _extract_claim(payload, USER_ID_CLAIM_KEYS)
    if not user_id:
        raise JwtValidationError("User id claim is missing")

    return User(user_id=user_id, role=_parse_role(payload))
