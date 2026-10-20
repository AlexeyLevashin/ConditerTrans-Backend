using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DataAccess.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260929000000_AddDispatcherProductRatingDailyMart")]
public sealed class AddDispatcherProductRatingDailyMart : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // One row per UTC creation day, production company and product name.
        // A product is counted once per order, even if the order has multiple matching lines.
        migrationBuilder.Sql("""
            CREATE MATERIALIZED VIEW dispatcher_product_rating_daily AS
            SELECT (o.creation_date AT TIME ZONE 'UTC')::date AS order_date,
                   p.company_id,
                   p.name AS product_name,
                   count(DISTINCT o.id)::integer AS order_count
            FROM orders o
            JOIN order_lines ol ON ol.order_id = o.id
            JOIN products p ON p.id = ol.product_id
            WHERE o.status = 2 or o.status = 5
              AND NOT EXISTS (
                  SELECT 1
                  FROM order_lines other_line
                  LEFT JOIN products other_product ON other_product.id = other_line.product_id
                  WHERE other_line.order_id = o.id
                    AND (other_product.id IS NULL OR other_product.company_id <> p.company_id)
              )
            GROUP BY (o.creation_date AT TIME ZONE 'UTC')::date, p.company_id, p.name;

            CREATE UNIQUE INDEX ux_dispatcher_product_rating_daily
                ON dispatcher_product_rating_daily (company_id, order_date, product_name);
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP MATERIALIZED VIEW dispatcher_product_rating_daily;");
    }
}
