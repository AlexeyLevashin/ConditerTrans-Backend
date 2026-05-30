from typing import Annotated
from fastapi import APIRouter, UploadFile, Depends, Query
from sqlalchemy.ext.asyncio import AsyncSession
from db.db import get_db
from repositories.file_repository import FileRepository
from services.file_service import FileService

router = APIRouter()


def file_service_depends(db: AsyncSession = Depends(get_db)):
    return FileService(FileRepository(db))


@router.post("/file")
async def upload_file(file: UploadFile, file_service: Annotated[FileService, Depends(file_service_depends)]):
    return await file_service.upload_file(file)


@router.get("/file/{file_id}")
async def get_file(file_id: str, file_service: Annotated[FileService, Depends(file_service_depends)]):
    return await file_service.get_file(file_id)


@router.get("/files")
async def get_files_by_ids(
    file_service: Annotated[FileService, Depends(file_service_depends)],
    file_ids: list[str] = Query([])
):
    return await file_service.get_files_by_ids(file_ids)
