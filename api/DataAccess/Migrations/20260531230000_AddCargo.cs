using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations;

/// <inheritdoc />
public partial class AddCargo : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("UPDATE orders SET cargo_id = NULL WHERE cargo_id IS NOT NULL;");

        migrationBuilder.CreateTable(
            name: "cargos",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                loading_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                unloading_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                delivery_address = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                volume = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                weight = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                status = table.Column<int>(type: "integer", nullable: false),
                logistic_company_id = table.Column<Guid>(type: "uuid", nullable: true),
                driver_id = table.Column<Guid>(type: "uuid", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_cargos", x => x.id);
                table.ForeignKey(
                    name: "FK_cargos_companies_logistic_company_id",
                    column: x => x.logistic_company_id,
                    principalTable: "companies",
                    principalColumn: "id",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "FK_cargos_users_driver_id",
                    column: x => x.driver_id,
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateTable(
            name: "cargo_change_histories",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                change_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                cargo_status = table.Column<int>(type: "integer", nullable: false),
                cargo_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_cargo_change_histories", x => x.id);
                table.ForeignKey(
                    name: "FK_cargo_change_histories_cargos_cargo_id",
                    column: x => x.cargo_id,
                    principalTable: "cargos",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_orders_cargo_id",
            table: "orders",
            column: "cargo_id",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_cargo_change_histories_cargo_id",
            table: "cargo_change_histories",
            column: "cargo_id");

        migrationBuilder.CreateIndex(
            name: "IX_cargos_driver_id",
            table: "cargos",
            column: "driver_id");

        migrationBuilder.CreateIndex(
            name: "IX_cargos_logistic_company_id",
            table: "cargos",
            column: "logistic_company_id");

        migrationBuilder.CreateIndex(
            name: "IX_cargos_status",
            table: "cargos",
            column: "status");

        migrationBuilder.AddForeignKey(
            name: "FK_orders_cargos_cargo_id",
            table: "orders",
            column: "cargo_id",
            principalTable: "cargos",
            principalColumn: "id",
            onDelete: ReferentialAction.SetNull);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_orders_cargos_cargo_id",
            table: "orders");

        migrationBuilder.DropTable(
            name: "cargo_change_histories");

        migrationBuilder.DropTable(
            name: "cargos");

        migrationBuilder.DropIndex(
            name: "IX_orders_cargo_id",
            table: "orders");
    }
}
