using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DataAccess.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261013000000_ReplaceProductRatingViewWithTable")]
public sealed class ReplaceProductRatingViewWithTable : Migration
{
    // One row per UTC order creation day, company and product name.
    // Different product IDs with the same name remain one rating item.
    private const string AggregateQuery = """
        SELECT (o.creation_date AT TIME ZONE 'UTC')::date AS order_date,
               p.company_id,
               p.name AS product_name,
               count(DISTINCT o.id)::integer AS order_count
        FROM public.orders o
        JOIN public.order_lines ol ON ol.order_id = o.id
        JOIN public.products p ON p.id = ol.product_id
        WHERE o.status IN (1, 2, 3, 5, 6, 7)
          AND NOT EXISTS (
              SELECT 1
              FROM public.order_lines other_line
              LEFT JOIN public.products other_product ON other_product.id = other_line.product_id
              WHERE other_line.order_id = o.id
                AND (other_product.id IS NULL OR other_product.company_id <> p.company_id)
          )
        GROUP BY (o.creation_date AT TIME ZONE 'UTC')::date, p.company_id, p.name
        """;

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql($"""
            DROP MATERIALIZED VIEW public.dispatcher_product_rating_daily;

            CREATE TABLE public.dispatcher_product_rating_daily (
                order_date date NOT NULL,
                company_id uuid NOT NULL,
                product_name varchar(256) NOT NULL,
                order_count integer NOT NULL CHECK (order_count > 0),
                CONSTRAINT pk_dispatcher_product_rating_daily
                    PRIMARY KEY (company_id, order_date, product_name)
            );

            CREATE FUNCTION public.refresh_dispatcher_product_rating_daily()
            RETURNS void
            LANGUAGE plpgsql
            AS $function$
            BEGIN
                -- Serialize refreshes, including calls made outside Hangfire.
                PERFORM pg_advisory_xact_lock(60101320261013::bigint);
                -- DELETE and INSERT share a transaction. Readers see the previous
                -- committed contents until the complete replacement commits.
                DELETE FROM public.dispatcher_product_rating_daily;
                INSERT INTO public.dispatcher_product_rating_daily
                    (order_date, company_id, product_name, order_count)
                {AggregateQuery};
            END;
            $function$;

            SELECT public.refresh_dispatcher_product_rating_daily();
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql($"""
            DROP FUNCTION public.refresh_dispatcher_product_rating_daily();
            DROP TABLE public.dispatcher_product_rating_daily;

            CREATE MATERIALIZED VIEW public.dispatcher_product_rating_daily AS
            {AggregateQuery};

            CREATE UNIQUE INDEX ux_dispatcher_product_rating_daily
                ON public.dispatcher_product_rating_daily (company_id, order_date, product_name);
            """);
    }
}
