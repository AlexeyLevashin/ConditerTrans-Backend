import uuid
from dataclasses import dataclass
from src.auth.user_role import UserRole


@dataclass(frozen=True)
class User:
    user_id: uuid.UUID
    role: UserRole

    def has_role(self, role: UserRole) -> bool:
        return self.role == role

    def has_any_role(self, *roles: UserRole) -> bool:
        return self.role in roles
