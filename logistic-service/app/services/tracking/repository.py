from datetime import UTC, datetime
from uuid import UUID

from sqlalchemy import func, select, text
from sqlalchemy.ext.asyncio import AsyncSession

from app.db.models.cargo_movement_history import CargoMovementHistory
from app.schemas.tracking import (
    CargoLocation,
    CargoMovementHistoryItem,
    CargoTrackingInfo,
    LocationUpdateRequest,
)

ACTIVE_CARGO_STATUSES = {1, 2}  # AwaitingTransportation, PickedUpFromProduction


class TrackingRepository:
    async def get_cargo_tracking_info(
        self,
        session: AsyncSession,
        cargo_id: UUID,
    ) -> CargoTrackingInfo | None:
        result = await session.execute(
            text(
                """
                SELECT id, driver_id, delivery_address, status
                FROM cargos
                WHERE id = :cargo_id
                """
            ),
            {"cargo_id": cargo_id},
        )
        row = result.first()
        if row is None:
            return None

        return CargoTrackingInfo(
            cargo_id=row.id,
            driver_id=row.driver_id,
            delivery_address=row.delivery_address,
            status=row.status,
        )

    async def save_location_update(
        self,
        session: AsyncSession,
        cargo_id: UUID,
        driver_id: UUID,
        payload: LocationUpdateRequest,
    ) -> CargoLocation:
        recorded_at = datetime.now(tz=UTC)
        movement = CargoMovementHistory(
            cargo_id=cargo_id,
            driver_id=driver_id,
            latitude=payload.latitude,
            longitude=payload.longitude,
            heading=payload.heading,
            speed=payload.speed,
            recorded_at=recorded_at,
        )
        session.add(movement)
        await session.commit()
        await session.refresh(movement)

        return CargoLocation(
            cargo_id=cargo_id,
            driver_id=driver_id,
            latitude=movement.latitude,
            longitude=movement.longitude,
            heading=movement.heading,
            speed=movement.speed,
            updated_at=movement.recorded_at,
        )

    async def get_latest_location(
        self,
        session: AsyncSession,
        cargo_id: UUID,
    ) -> CargoLocation | None:
        stmt = (
            select(CargoMovementHistory)
            .where(CargoMovementHistory.cargo_id == cargo_id)
            .order_by(CargoMovementHistory.recorded_at.desc())
            .limit(1)
        )
        result = await session.execute(stmt)
        movement = result.scalar_one_or_none()
        if movement is None:
            return None

        return CargoLocation(
            cargo_id=movement.cargo_id,
            driver_id=movement.driver_id,
            latitude=movement.latitude,
            longitude=movement.longitude,
            heading=movement.heading,
            speed=movement.speed,
            updated_at=movement.recorded_at,
        )

    async def get_history(
        self,
        session: AsyncSession,
        cargo_id: UUID,
        limit: int,
        offset: int,
    ) -> tuple[list[CargoMovementHistoryItem], int]:
        count_stmt = (
            select(func.count())
            .select_from(CargoMovementHistory)
            .where(CargoMovementHistory.cargo_id == cargo_id)
        )
        total = int((await session.execute(count_stmt)).scalar_one())

        stmt = (
            select(CargoMovementHistory)
            .where(CargoMovementHistory.cargo_id == cargo_id)
            .order_by(CargoMovementHistory.recorded_at.asc())
            .offset(offset)
            .limit(limit)
        )
        result = await session.execute(stmt)
        items = [
            CargoMovementHistoryItem(
                id=movement.id,
                cargo_id=movement.cargo_id,
                driver_id=movement.driver_id,
                latitude=movement.latitude,
                longitude=movement.longitude,
                heading=movement.heading,
                speed=movement.speed,
                recorded_at=movement.recorded_at,
            )
            for movement in result.scalars().all()
        ]
        return items, total


tracking_repository = TrackingRepository()
