from sqlalchemy import select

from app.db.models.category import Category
from app.db.repositories.base import BaseRepository


class CategoryRepository(BaseRepository):
    async def get_all(self) -> list[Category]:
        result = await self.db.scalars(
            select(Category).order_by(Category.id),
        )
        return list(result.all())
