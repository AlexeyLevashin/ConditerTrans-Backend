from sqlalchemy.orm import Mapped, mapped_column
from app.db.database import Base
from sqlalchemy import ForeignKey, String, Uuid
from enum import Enum


class UnitOfProduct(Enum):
    kilograms = "кг"
    grams = "гр"
    milliliters = "мл"
    liters = "л"


class Product(Base):
    __tablename__ = "products"

    id: Mapped[int] = mapped_column(primary_key=True, autoincrement=True)
    name: Mapped[str] = mapped_column(String(255), nullable=False)
    description: Mapped[str] = mapped_column(nullable=True)
    price: Mapped[float] = mapped_column(nullable=False)
    count: Mapped[float] = mapped_column(nullable=False)
    unit: Mapped[UnitOfProduct] = mapped_column(nullable=False)
    expiry_time: Mapped[int] = mapped_column(nullable=False)
    category_id: Mapped[int] = mapped_column(ForeignKey("categories.id"), nullable=False)
    company_id: Mapped[Uuid] = mapped_column(nullable=False)
