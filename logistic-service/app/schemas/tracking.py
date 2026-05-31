from datetime import datetime
from uuid import UUID

from pydantic import BaseModel, Field


class CargoTrackingInfo(BaseModel):
    cargo_id: UUID
    driver_id: UUID | None
    delivery_address: str
    status: int


class CargoLocation(BaseModel):
    cargo_id: UUID
    driver_id: UUID
    latitude: float
    longitude: float
    heading: float | None = None
    speed: float | None = None
    updated_at: datetime


class LocationUpdateRequest(BaseModel):
    latitude: float = Field(ge=-90, le=90)
    longitude: float = Field(ge=-180, le=180)
    heading: float | None = Field(default=None, ge=0, le=360)
    speed: float | None = Field(default=None, ge=0)


class LocationMessage(BaseModel):
    type: str = "location"
    cargo_id: UUID
    driver_id: UUID
    latitude: float
    longitude: float
    heading: float | None = None
    speed: float | None = None
    updated_at: str


class CargoMovementHistoryItem(BaseModel):
    id: UUID
    cargo_id: UUID
    driver_id: UUID
    latitude: float
    longitude: float
    heading: float | None = None
    speed: float | None = None
    recorded_at: datetime


class CargoMovementHistoryResponse(BaseModel):
    cargo_id: UUID
    items: list[CargoMovementHistoryItem]
    total: int
