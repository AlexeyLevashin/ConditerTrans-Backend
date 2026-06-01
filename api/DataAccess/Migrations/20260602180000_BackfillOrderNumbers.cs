using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class BackfillOrderNumbers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                WITH to_number AS (
                    SELECT
                        id,
                        ROW_NUMBER() OVER (ORDER BY creation_date, id) AS row_num
                    FROM orders
                    WHERE order_number <= 0 AND status <> 0
                ),
                max_num AS (
                    SELECT COALESCE(MAX(order_number), 0) AS value FROM orders WHERE order_number > 0
                )
                UPDATE orders o
                SET order_number = max_num.value + to_number.row_num
                FROM to_number, max_num
                WHERE o.id = to_number.id;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
