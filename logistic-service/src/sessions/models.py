import datetime
import uuid
from sqlalchemy.orm import MappedColumn, Mapped
from database.core.database import Base


class Session(Base):
    id: Mapped[uuid.UUID] = MappedColumn(default_factory=uuid.uuid4, primary_key=True)
    user_id: Mapped[uuid.UUID] = MappedColumn(nullable=False)
    start_time: Mapped[datetime.datetime] = MappedColumn(nullable=False)
    end_time: Mapped[datetime.datetime] = MappedColumn(nullable=True)
