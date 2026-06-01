using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderShipmentDimensions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "shipment_height_m",
                table: "orders",
                type: "numeric(18,3)",
                precision: 18,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "shipment_length_m",
                table: "orders",
                type: "numeric(18,3)",
                precision: 18,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "shipment_weight_kg",
                table: "orders",
                type: "numeric(18,3)",
                precision: 18,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "shipment_width_m",
                table: "orders",
                type: "numeric(18,3)",
                precision: 18,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "dimensions",
                table: "cargos",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "shipment_height_m",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "shipment_length_m",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "shipment_weight_kg",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "shipment_width_m",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "dimensions",
                table: "cargos");
        }
    }
}
