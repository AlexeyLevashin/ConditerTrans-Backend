import uuid
from dataclasses import dataclass

@dataclass
class ProductResponse:
    id: uuid.UUID
    nomenclature_number: str
    name: str
    description: str
    price: float
    count: float
    expiry_time: int
