from repositories.base import BaseRepository
from sqlalchemy import insert, select
from db.db import async_session_maker
from models.file import File
from typing import Optional
from uuid import UUID


class FileRepository(BaseRepository):
    async def add_file(self, file_name: str, file_id: UUID, extension: str):
        await self.db.execute(insert(File).values(
            id=file_id,
            name=file_name,
            extension=extension
        ))
        await self.db.commit()

    async def get_file_by_id(self, file_id: str) -> Optional[File]:
        file = await self.db.execute(select(File).where(
            File.id == file_id
        ))
        res = file.one_or_none()
        return res[0] if res else None

    async def get_files_by_ids(self, file_ids: list[str]) -> list[File]:
        result = await self.db.execute(
            select(File).where(File.id.in_(file_ids))
        )
        files = result.scalars().all()
        return files
