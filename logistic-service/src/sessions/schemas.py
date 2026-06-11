from pydantic import BaseModel
from datetime import datetime
import uuid


class SessionResponse(BaseModel):
    id: uuid.UUID
    user_id: uuid.UUID
    start_time: datetime
    end_time: datetime | None
