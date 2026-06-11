import datetime
import uuid
from sqlalchemy.orm import Mapped, mapped_column
from src.database.core.database import Base


class Session(Base):
    __tablename__ = "sessions"

    id: Mapped[uuid.UUID] = mapped_column(default=uuid.uuid4, primary_key=True)
    user_id: Mapped[uuid.UUID] = mapped_column(nullable=False)
    start_time: Mapped[datetime.datetime] = mapped_column(nullable=False)
    end_time: Mapped[datetime.datetime] = mapped_column(nullable=True)
