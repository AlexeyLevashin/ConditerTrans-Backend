import json
from typing import Annotated
from uuid import UUID

from fastapi import APIRouter, Depends, HTTPException, Query, WebSocket, WebSocketDisconnect, status
from sqlalchemy.ext.asyncio import AsyncSession

from app.db.database import get_db
from app.schemas.tracking import (
    CargoLocation,
    CargoMovementHistoryResponse,
    LocationUpdateRequest,
)
from app.services.tracking.manager import tracking_manager
from app.services.tracking.repository import ACTIVE_CARGO_STATUSES, tracking_repository

router = APIRouter(prefix="/tracking", tags=["tracking"])


async def _get_trackable_cargo(session: AsyncSession, cargo_id: UUID):
    cargo = await tracking_repository.get_cargo_tracking_info(session, cargo_id)
    if cargo is None:
        raise HTTPException(status_code=status.HTTP_404_NOT_FOUND, detail="Cargo not found")
    if cargo.status not in ACTIVE_CARGO_STATUSES:
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail="Cargo is not available for tracking",
        )
    if cargo.driver_id is None:
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail="Driver is not assigned to cargo",
        )
    return cargo


@router.get("/cargo/{cargo_id}/location", response_model=CargoLocation)
async def get_cargo_location(
    cargo_id: UUID,
    session: Annotated[AsyncSession, Depends(get_db)],
) -> CargoLocation:
    await _get_trackable_cargo(session, cargo_id)

    location = await tracking_repository.get_latest_location(session, cargo_id)
    if location is None:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Location for cargo is not available yet",
        )

    return location


@router.get("/cargo/{cargo_id}/history", response_model=CargoMovementHistoryResponse)
async def get_cargo_movement_history(
    cargo_id: UUID,
    session: Annotated[AsyncSession, Depends(get_db)],
    limit: Annotated[int, Query(ge=1, le=5000)] = 500,
    offset: Annotated[int, Query(ge=0)] = 0,
) -> CargoMovementHistoryResponse:
    await _get_trackable_cargo(session, cargo_id)

    items, total = await tracking_repository.get_history(session, cargo_id, limit, offset)
    return CargoMovementHistoryResponse(cargo_id=cargo_id, items=items, total=total)


@router.websocket("/ws/cargo/{cargo_id}")
async def cargo_tracking_websocket(
    websocket: WebSocket,
    cargo_id: UUID,
    role: Annotated[str, Query()] = "subscriber",
    driver_id: Annotated[str | None, Query()] = None,
) -> None:
    from app.db.database import AsyncSessionLocal

    async with AsyncSessionLocal() as session:
        try:
            cargo = await _get_trackable_cargo(session, cargo_id)
        except HTTPException as exc:
            await websocket.close(code=4404 if exc.status_code == 404 else 4400, reason=exc.detail)
            return

    await websocket.accept()

    if role == "driver":
        if cargo.driver_id is None:
            await websocket.close(code=4400, reason="Driver is not assigned to cargo")
            return

        if driver_id is None:
            await websocket.close(code=4400, reason="driver_id query parameter is required")
            return

        try:
            resolved_driver_id = UUID(driver_id)
        except ValueError:
            await websocket.close(code=4400, reason="Invalid driver_id")
            return

        if resolved_driver_id != cargo.driver_id:
            await websocket.send_json(
                {
                    "type": "error",
                    "message": f"Driver '{driver_id}' is not assigned to cargo '{cargo_id}'",
                }
            )
            await websocket.close(code=4403, reason="Driver is not assigned to cargo")
            return

        await tracking_manager.register_driver(cargo_id, websocket)
        await websocket.send_json(
            {
                "type": "connected",
                "role": "driver",
                "cargo_id": str(cargo_id),
                "driver_id": str(resolved_driver_id),
                "delivery_address": cargo.delivery_address,
            }
        )

        async with AsyncSessionLocal() as session:
            current_location = await tracking_repository.get_latest_location(session, cargo_id)
        if current_location is not None:
            await tracking_manager.send_location(websocket, current_location)

        try:
            while True:
                raw = await websocket.receive_text()
                payload = json.loads(raw)

                if payload.get("type") != "location_update":
                    await websocket.send_json(
                        {
                            "type": "error",
                            "message": "Unsupported message type. Use 'location_update'.",
                        }
                    )
                    continue

                update = LocationUpdateRequest.model_validate(payload)
                async with AsyncSessionLocal() as session:
                    location = await tracking_repository.save_location_update(
                        session,
                        cargo_id,
                        resolved_driver_id,
                        update,
                    )
                await tracking_manager.broadcast_location(cargo_id, location)
                await websocket.send_json(
                    {
                        "type": "location_ack",
                        "updated_at": location.updated_at.isoformat(),
                    }
                )
        except WebSocketDisconnect:
            await tracking_manager.unregister_driver(cargo_id, websocket)
        except Exception as exc:
            await tracking_manager.unregister_driver(cargo_id, websocket)
            await websocket.close(code=1011, reason=str(exc))
        return

    await tracking_manager.add_subscriber(cargo_id, websocket)
    await websocket.send_json(
        {
            "type": "connected",
            "role": "subscriber",
            "cargo_id": str(cargo_id),
            "driver_id": str(cargo.driver_id),
            "delivery_address": cargo.delivery_address,
        }
    )

    async with AsyncSessionLocal() as session:
        current_location = await tracking_repository.get_latest_location(session, cargo_id)
    if current_location is not None:
        await tracking_manager.send_location(websocket, current_location)

    try:
        while True:
            await websocket.receive_text()
    except WebSocketDisconnect:
        await tracking_manager.remove_subscriber(cargo_id, websocket)
