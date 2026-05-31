from collections import defaultdict

from fastapi import WebSocket

from app.schemas.tracking import LocationMessage, TripLocation


class TrackingConnectionManager:
    def __init__(self) -> None:
        self._subscribers: dict[str, set[WebSocket]] = defaultdict(set)
        self._drivers: dict[str, WebSocket] = {}

    async def add_subscriber(self, trip_id: str, websocket: WebSocket) -> None:
        self._subscribers[trip_id].add(websocket)

    async def remove_subscriber(self, trip_id: str, websocket: WebSocket) -> None:
        self._subscribers[trip_id].discard(websocket)
        if not self._subscribers[trip_id]:
            self._subscribers.pop(trip_id, None)

    async def register_driver(self, trip_id: str, websocket: WebSocket) -> None:
        previous = self._drivers.get(trip_id)
        if previous is not None and previous is not websocket:
            await previous.close(code=4000, reason="Another driver connected to this trip")
        self._drivers[trip_id] = websocket

    async def unregister_driver(self, trip_id: str, websocket: WebSocket) -> None:
        if self._drivers.get(trip_id) is websocket:
            self._drivers.pop(trip_id, None)

    @staticmethod
    def location_to_message(location: TripLocation) -> dict:
        return LocationMessage(
            trip_id=location.trip_id,
            employee_id=location.employee_id,
            latitude=location.latitude,
            longitude=location.longitude,
            heading=location.heading,
            speed=location.speed,
            updated_at=location.updated_at.isoformat(),
        ).model_dump()

    async def send_location(self, websocket: WebSocket, location: TripLocation) -> None:
        await websocket.send_json(self.location_to_message(location))

    async def broadcast_location(self, trip_id: str, location: TripLocation) -> None:
        message = self.location_to_message(location)
        dead: list[WebSocket] = []

        for websocket in self._subscribers.get(trip_id, set()):
            try:
                await websocket.send_json(message)
            except Exception:
                dead.append(websocket)

        for websocket in dead:
            await self.remove_subscriber(trip_id, websocket)

        driver = self._drivers.get(trip_id)
        if driver is not None:
            try:
                await driver.send_json(message)
            except Exception:
                await self.unregister_driver(trip_id, driver)


tracking_manager = TrackingConnectionManager()
