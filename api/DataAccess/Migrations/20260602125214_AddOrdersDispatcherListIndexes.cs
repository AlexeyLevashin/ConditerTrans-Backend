using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddOrdersDispatcherListIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_orders_creation_date",
                table: "orders",
                column: "creation_date");

            migrationBuilder.CreateIndex(
                name: "IX_orders_status_creation_date",
                table: "orders",
                columns: new[] { "status", "creation_date" });

            migrationBuilder.CreateIndex(
                name: "IX_orders_order_number",
                table: "orders",
                column: "order_number");

            migrationBuilder.CreateIndex(
                name: "IX_order_change_histories_order_status_change_time",
                table: "order_change_histories",
                columns: new[] { "order_status", "change_time" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_order_change_histories_order_status_change_time",
                table: "order_change_histories");

            migrationBuilder.DropIndex(
                name: "IX_orders_order_number",
                table: "orders");

            migrationBuilder.DropIndex(
                name: "IX_orders_status_creation_date",
                table: "orders");

            migrationBuilder.DropIndex(
                name: "IX_orders_creation_date",
                table: "orders");
        }
    }
}
