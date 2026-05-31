from uuid import UUID

from sqlalchemy import select

from app.db.models.product import Product
from app.db.repositories import BaseRepository


class ProductRepository(BaseRepository):
    async def get_products_by_company_id(self, company_id: UUID) -> list[Product]:
        result = await self.db.scalars(
            select(Product).where(Product.company_id == company_id)
        )
        return list(result.all())
