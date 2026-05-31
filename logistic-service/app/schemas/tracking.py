from datetime import datetime

from pydantic import BaseModel, Field


class TripAssignment(BaseModel):
    trip_id: str
    employee_id: str
    employee_name: str


class TripLocation(BaseModel):
    trip_id: str
    employee_id: str
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
    trip_id: str
    employee_id: str
    latitude: float
    longitude: float
    heading: float | None = None
    speed: float | None = None
    updated_at: str
