from app.db.repositories.category.repository import CategoryRepository
from app.schemas.categories import CategoryResponse


class CategoryService:
    def __init__(self, repo: CategoryRepository) -> None:
        self.repo = repo

    async def get_all(self) -> list[CategoryResponse]:
        categories = await self.repo.get_all()
        return [
            CategoryResponse(id=category.id, name=category.name)
            for category in categories
        ]
