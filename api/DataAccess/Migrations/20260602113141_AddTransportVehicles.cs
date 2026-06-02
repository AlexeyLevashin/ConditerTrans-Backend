using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddTransportVehicles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "transport_vehicle_id",
                table: "cargos",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "vehicle_brands",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicle_brands", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "vehicle_models",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    brand_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicle_models", x => x.id);
                    table.ForeignKey(
                        name: "FK_vehicle_models_vehicle_brands_brand_id",
                        column: x => x.brand_id,
                        principalTable: "vehicle_brands",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "transport_vehicles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    registration_number = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    capacity = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    model_id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transport_vehicles", x => x.id);
                    table.ForeignKey(
                        name: "FK_transport_vehicles_companies_company_id",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_transport_vehicles_employees_employee_id",
                        column: x => x.employee_id,
                        principalTable: "employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_transport_vehicles_vehicle_models_model_id",
                        column: x => x.model_id,
                        principalTable: "vehicle_models",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_cargos_transport_vehicle_id",
                table: "cargos",
                column: "transport_vehicle_id");

            migrationBuilder.CreateIndex(
                name: "IX_transport_vehicles_company_id_registration_number",
                table: "transport_vehicles",
                columns: new[] { "company_id", "registration_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_transport_vehicles_employee_id",
                table: "transport_vehicles",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_transport_vehicles_model_id",
                table: "transport_vehicles",
                column: "model_id");

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_brands_name",
                table: "vehicle_brands",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_models_brand_id_name",
                table: "vehicle_models",
                columns: new[] { "brand_id", "name" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_cargos_transport_vehicles_transport_vehicle_id",
                table: "cargos",
                column: "transport_vehicle_id",
                principalTable: "transport_vehicles",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.Sql(
                """
                INSERT INTO vehicle_brands (id, name) VALUES
                  ('11111111-1111-1111-1111-111111111101', 'КАМАЗ'),
                  ('11111111-1111-1111-1111-111111111102', 'МАЗ'),
                  ('11111111-1111-1111-1111-111111111103', 'Scania');

                INSERT INTO vehicle_models (id, name, brand_id) VALUES
                  ('22222222-2222-2222-2222-222222222201', '65115', '11111111-1111-1111-1111-111111111101'),
                  ('22222222-2222-2222-2222-222222222202', '5337', '11111111-1111-1111-1111-111111111102'),
                  ('22222222-2222-2222-2222-222222222203', 'R450', '11111111-1111-1111-1111-111111111103');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_cargos_transport_vehicles_transport_vehicle_id",
                table: "cargos");

            migrationBuilder.DropTable(
                name: "transport_vehicles");

            migrationBuilder.DropTable(
                name: "vehicle_models");

            migrationBuilder.DropTable(
                name: "vehicle_brands");

            migrationBuilder.DropIndex(
                name: "IX_cargos_transport_vehicle_id",
                table: "cargos");

            migrationBuilder.DropColumn(
                name: "transport_vehicle_id",
                table: "cargos");
        }
    }
}
