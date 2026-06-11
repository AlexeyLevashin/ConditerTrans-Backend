import uuid
from typing import TypeVar, Generic, List, Optional, Any
from sqlalchemy import insert, select, update
from sqlalchemy.ext.asyncio import AsyncSession
from src.database.core.database import Base

T = TypeVar('T', bound=Base)


class SQLAlchemyRepository(Generic[T]):
    model: T = None

    def __init__(self, session: AsyncSession):
        self._session: AsyncSession = session

    async def add_one(self, **data: Any) -> T:
        stmt = insert(self.model).values(**data).returning(self.model)
        res = await self._session.execute(stmt)
        await self._session.commit()
        return res.scalar_one()

    async def edit_one_by_id(self, model_id: uuid.UUID, **data) -> T:
        stmt = (
            update(self.model)
            .where(self.model.id == model_id)
            .values(**data)
            .returning(self.model)
        )
        res = await self._session.execute(stmt)
        await self._session.commit()
        return res.scalar_one()

    async def find_all(self) -> List[T]:
        stmt = select(self.model)
        res = await self._session.execute(stmt)
        return list(res.scalars().all())

    async def find_one(self, **filter_by: Any) -> Optional[T]:
        stmt = select(self.model).filter_by(**filter_by)
        res = await self._session.execute(stmt)
        return res.scalar_one_or_none()
