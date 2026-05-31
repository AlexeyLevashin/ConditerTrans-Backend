from datetime import UTC, datetime

from app.schemas.tracking import LocationUpdateRequest, TripAssignment, TripLocation

MOCK_TRIP_ASSIGNMENTS: dict[str, TripAssignment] = {
    "R00101": TripAssignment(
        trip_id="R00101",
        employee_id="emp-driver-001",
        employee_name="Иванов Петр Сергеевич",
    ),
}

MOCK_TRIP_LOCATIONS: dict[str, TripLocation] = {
    "R00101": TripLocation(
        trip_id="R00101",
        employee_id="emp-driver-001",
        latitude=55.872,
        longitude=37.392,
        heading=315.0,
        speed=68.0,
        updated_at=datetime.now(tz=UTC),
    ),
}


class TrackingStore:
    def get_assignment(self, trip_id: str) -> TripAssignment | None:
        return MOCK_TRIP_ASSIGNMENTS.get(trip_id)

    def get_location(self, trip_id: str) -> TripLocation | None:
        return MOCK_TRIP_LOCATIONS.get(trip_id)

    def list_assignments(self) -> list[TripAssignment]:
        return list(MOCK_TRIP_ASSIGNMENTS.values())

    def update_location(
        self,
        trip_id: str,
        employee_id: str,
        payload: LocationUpdateRequest,
    ) -> TripLocation:
        assignment = self.get_assignment(trip_id)
        if assignment is None:
            raise KeyError(f"Trip '{trip_id}' is not found in mock assignments")

        if assignment.employee_id != employee_id:
            raise PermissionError(
                f"Employee '{employee_id}' is not assigned to trip '{trip_id}'"
            )

        location = TripLocation(
            trip_id=trip_id,
            employee_id=employee_id,
            latitude=payload.latitude,
            longitude=payload.longitude,
            heading=payload.heading,
            speed=payload.speed,
            updated_at=datetime.now(tz=UTC),
        )
        MOCK_TRIP_LOCATIONS[trip_id] = location
        return location


tracking_store = TrackingStore()
