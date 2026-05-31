from dataclasses import dataclass

from app.enums.user_role import UserRole


@dataclass(frozen=True)
class CurrentUser:
    user_id: str
    role: UserRole

    def has_role(self, role: UserRole) -> bool:
        return self.role == role

    def has_any_role(self, *roles: UserRole) -> bool:
        return self.role in roles
