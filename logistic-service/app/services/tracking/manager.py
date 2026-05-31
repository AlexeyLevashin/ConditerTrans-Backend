from collections import defaultdict
from uuid import UUID

from fastapi import WebSocket

from app.schemas.tracking import CargoLocation, LocationMessage


class TrackingConnectionManager:
    def __init__(self) -> None:
        self._subscribers: dict[str, set[WebSocket]] = defaultdict(set)
        self._drivers: dict[str, WebSocket] = {}

    @staticmethod
    def _key(cargo_id: UUID) -> str:
        return str(cargo_id)

    async def add_subscriber(self, cargo_id: UUID, websocket: WebSocket) -> None:
        self._subscribers[self._key(cargo_id)].add(websocket)

    async def remove_subscriber(self, cargo_id: UUID, websocket: WebSocket) -> None:
        key = self._key(cargo_id)
        self._subscribers[key].discard(websocket)
        if not self._subscribers[key]:
            self._subscribers.pop(key, None)

    async def register_driver(self, cargo_id: UUID, websocket: WebSocket) -> None:
        key = self._key(cargo_id)
        previous = self._drivers.get(key)
        if previous is not None and previous is not websocket:
            await previous.close(code=4000, reason="Another driver connected to this cargo")
        self._drivers[key] = websocket

    async def unregister_driver(self, cargo_id: UUID, websocket: WebSocket) -> None:
        key = self._key(cargo_id)
        if self._drivers.get(key) is websocket:
            self._drivers.pop(key, None)

    @staticmethod
    def location_to_message(location: CargoLocation) -> dict:
        return LocationMessage(
            cargo_id=location.cargo_id,
            driver_id=location.driver_id,
            latitude=location.latitude,
            longitude=location.longitude,
            heading=location.heading,
            speed=location.speed,
            updated_at=location.updated_at.isoformat(),
        ).model_dump()

    async def send_location(self, websocket: WebSocket, location: CargoLocation) -> None:
        await websocket.send_json(
            {
                "type": "location",
                **self.location_to_message(location),
            }
        )

    async def broadcast_location(self, cargo_id: UUID, location: CargoLocation) -> None:
        message = {
            "type": "location",
            **self.location_to_message(location),
        }
        key = self._key(cargo_id)
        dead: list[WebSocket] = []

        for websocket in self._subscribers.get(key, set()):
            try:
                await websocket.send_json(message)
            except Exception:
                dead.append(websocket)

        for websocket in dead:
            await self.remove_subscriber(cargo_id, websocket)

        driver = self._drivers.get(key)
        if driver is not None:
            try:
                await driver.send_json(message)
            except Exception:
                await self.unregister_driver(cargo_id, driver)


tracking_manager = TrackingConnectionManager()
