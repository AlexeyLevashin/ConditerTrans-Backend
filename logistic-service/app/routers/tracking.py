import json
from typing import Annotated

from fastapi import APIRouter, HTTPException, Query, WebSocket, WebSocketDisconnect, status

from app.schemas.tracking import LocationUpdateRequest, TripAssignment, TripLocation
from app.services.tracking.manager import tracking_manager
from app.services.tracking.store import tracking_store

router = APIRouter(prefix="/tracking", tags=["tracking"])


@router.get("/trips", response_model=list[TripAssignment])
async def list_trip_assignments() -> list[TripAssignment]:
    return tracking_store.list_assignments()


@router.get("/trips/{trip_id}/location", response_model=TripLocation)
async def get_trip_location(trip_id: str) -> TripLocation:
    assignment = tracking_store.get_assignment(trip_id)
    if assignment is None:
        raise HTTPException(status_code=status.HTTP_404_NOT_FOUND, detail="Trip not found")

    location = tracking_store.get_location(trip_id)
    if location is None:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Location for trip is not available yet",
        )

    return location


@router.websocket("/ws/trips/{trip_id}")
async def trip_tracking_websocket(
    websocket: WebSocket,
    trip_id: str,
    role: Annotated[str, Query()] = "subscriber",
    employee_id: Annotated[str | None, Query()] = None,
) -> None:
    assignment = tracking_store.get_assignment(trip_id)
    if assignment is None:
        await websocket.close(code=4404, reason="Trip not found")
        return

    await websocket.accept()

    if role == "driver":
        resolved_employee_id = employee_id or assignment.employee_id
        if resolved_employee_id != assignment.employee_id:
            await websocket.send_json(
                {
                    "type": "error",
                    "message": f"Employee '{resolved_employee_id}' is not assigned to trip '{trip_id}'",
                }
            )
            await websocket.close(code=4403, reason="Employee is not assigned to trip")
            return

        await tracking_manager.register_driver(trip_id, websocket)
        await websocket.send_json(
            {
                "type": "connected",
                "role": "driver",
                "trip_id": trip_id,
                "employee_id": resolved_employee_id,
            }
        )

        current_location = tracking_store.get_location(trip_id)
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
                location = tracking_store.update_location(
                    trip_id,
                    resolved_employee_id,
                    update,
                )
                await tracking_manager.broadcast_location(trip_id, location)
                await websocket.send_json(
                    {
                        "type": "location_ack",
                        "updated_at": location.updated_at.isoformat(),
                    }
                )
        except WebSocketDisconnect:
            await tracking_manager.unregister_driver(trip_id, websocket)
        except Exception as exc:
            await tracking_manager.unregister_driver(trip_id, websocket)
            await websocket.close(code=1011, reason=str(exc))
        return

    await tracking_manager.add_subscriber(trip_id, websocket)
    await websocket.send_json(
        {
            "type": "connected",
            "role": "subscriber",
            "trip_id": trip_id,
            "employee_id": assignment.employee_id,
            "employee_name": assignment.employee_name,
        }
    )

    current_location = tracking_store.get_location(trip_id)
    if current_location is not None:
        await tracking_manager.send_location(websocket, current_location)

    try:
        while True:
            await websocket.receive_text()
    except WebSocketDisconnect:
        await tracking_manager.remove_subscriber(trip_id, websocket)
