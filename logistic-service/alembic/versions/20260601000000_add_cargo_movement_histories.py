"""add cargo movement histories

Revision ID: 20260601000000
Revises:
Create Date: 2026-06-01
"""

from typing import Sequence, Union

import sqlalchemy as sa
from alembic import op

revision: str = "20260601000000"
down_revision: Union[str, Sequence[str], None] = None
branch_labels: Union[str, Sequence[str], None] = None
depends_on: Union[str, Sequence[str], None] = None


def upgrade() -> None:
    op.create_table(
        "cargo_movement_histories",
        sa.Column("id", sa.Uuid(), nullable=False),
        sa.Column("cargo_id", sa.Uuid(), nullable=False),
        sa.Column("driver_id", sa.Uuid(), nullable=False),
        sa.Column("latitude", sa.Float(), nullable=False),
        sa.Column("longitude", sa.Float(), nullable=False),
        sa.Column("heading", sa.Float(), nullable=True),
        sa.Column("speed", sa.Float(), nullable=True),
        sa.Column(
            "recorded_at",
            sa.DateTime(timezone=True),
            server_default=sa.text("now()"),
            nullable=False,
        ),
        sa.ForeignKeyConstraint(["cargo_id"], ["cargos.id"], ondelete="CASCADE"),
        sa.PrimaryKeyConstraint("id"),
    )
    op.create_index(
        "ix_cargo_movement_histories_cargo_id",
        "cargo_movement_histories",
        ["cargo_id"],
        unique=False,
    )
    op.create_index(
        "ix_cargo_movement_histories_driver_id",
        "cargo_movement_histories",
        ["driver_id"],
        unique=False,
    )
    op.create_index(
        "ix_cargo_movement_histories_cargo_id_recorded_at",
        "cargo_movement_histories",
        ["cargo_id", "recorded_at"],
        unique=False,
    )


def downgrade() -> None:
    op.drop_index(
        "ix_cargo_movement_histories_cargo_id_recorded_at",
        table_name="cargo_movement_histories",
    )
    op.drop_index("ix_cargo_movement_histories_driver_id", table_name="cargo_movement_histories")
    op.drop_index("ix_cargo_movement_histories_cargo_id", table_name="cargo_movement_histories")
    op.drop_table("cargo_movement_histories")
