from uuid import UUID
from app.db.models.product import Product
from app.db.repositories.product.repository import ProductRepository
from app.schemas.products import ProductResponse


class ProductService:
    def __init__(self, repo: ProductRepository) -> None:
        self.repo = repo

    async def get_all(self, company_id: UUID) -> list[ProductResponse]:
        categories = await self.repo.get_products_by_company_id(company_id)
        return [
            ProductResponse(
                id=category.id,
                name=category.name
                            )
            for category in categories
        ]

    @staticmethod
    def __map_db_to_response(products: list[Product]) -> list[ProductResponse]:
        return [
            ProductResponse(
                id=product.id,
                nomenclature_number=str(product.id),
                name=product.name,
                description=product.description or "",
                price=product.price,
                count=product.count,
                expiry_time=product.expiry_time or 0,
            )
            for product in products
        ]