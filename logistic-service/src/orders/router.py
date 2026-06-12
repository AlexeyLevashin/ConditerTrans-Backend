from typing import Any, Annotated
from fastapi import APIRouter, Depends
from starlette.websockets import WebSocket, WebSocketDisconnect
from src.auth import require_roles_ws
from src.auth.schemas import User
from src.auth.user_role import UserRole
from src.orders.schemas import OrderNotify


class ConnectionManager:
    def __init__(self):
        self.active_connections: list[WebSocket] = []

    async def connect(self, websocket: WebSocket):
        await websocket.accept()
        self.active_connections.append(websocket)

    def disconnect(self, websocket: WebSocket):
        self.active_connections.remove(websocket)

    async def send_personal_message(self, message: str, websocket: WebSocket):
        await websocket.send_text(message)

    async def broadcast(self, message: Any):
        for connection in self.active_connections:
            await connection.send_json(message)


manager = ConnectionManager()
router = APIRouter(tags=["orders"])


@router.websocket("/ws/orders/new")
async def new_orders(
    websocket: WebSocket,
    user: Annotated[User, Depends(require_roles_ws(UserRole.COORDINATOR))],
):
    await manager.connect(websocket)
    try:
        while True:
            res = await websocket.receive_json()
            await manager.broadcast(res)
    except WebSocketDisconnect:
        manager.disconnect(websocket)


@router.post("/orders/notify")
async def notify_order(order: OrderNotify):
    await manager.broadcast(order.model_dump(mode="json"))


@router.post("/orders")
async def 