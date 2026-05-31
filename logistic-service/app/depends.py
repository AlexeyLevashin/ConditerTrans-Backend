from fastapi import Depends
from sqlalchemy.ext.asyncio import AsyncSession

from app.db.database import get_db
from app.db.repositories.category.repository import CategoryRepository
from app.services.category.service import CategoryService


async def get_category_repository(
    db: AsyncSession = Depends(get_db),
) -> CategoryRepository:
    return CategoryRepository(db)


async def get_category_service(
    repo: CategoryRepository = Depends(get_category_repository),
) -> CategoryService:
    return CategoryService(repo)
