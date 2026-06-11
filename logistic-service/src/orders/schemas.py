import uuid

from pydantic import BaseModel


class OrderNotify(BaseModel):
    id: uuid.UUID
