from typing import Annotated

from fastapi import APIRouter, Depends

from app.auth.dependencies import get_current_user
from app.depends import get_category_service
from app.schemas.auth import CurrentUser
from app.schemas.categories import CategoryResponse
from app.services.category.service import CategoryService

router = APIRouter(prefix="/categories", tags=["categories"])


@router.get(
    "",
    response_model=list[CategoryResponse],
    openapi_extra={"security": [{"BearerAuth": []}]},
)
async def get_categories(
    _: Annotated[CurrentUser, Depends(get_current_user)],
    service: CategoryService = Depends(get_category_service),
) -> list[CategoryResponse]:
    return await service.get_all()
