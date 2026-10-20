using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddRejectionReasons : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "rejection_reason_id",
                table: "order_change_histories",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "rejection_reasons",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: true),
                    name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    is_default = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rejection_reasons", x => x.id);
                    table.CheckConstraint("ck_rejection_reasons_scope", "(is_default = TRUE AND company_id IS NULL) OR (is_default = FALSE AND company_id IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_rejection_reasons_companies_company_id",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "rejection_reasons",
                columns: new[] { "id", "company_id", "created_at", "is_active", "is_default", "name" },
                values: new object[,]
                {
                    { new Guid("10000000-0000-0000-0000-000000000001"), null, new DateTime(2026, 9, 15, 0, 0, 0, 0, DateTimeKind.Utc), true, true, "Нехватка ресурсов" },
                    { new Guid("10000000-0000-0000-0000-000000000002"), null, new DateTime(2026, 9, 15, 0, 0, 0, 0, DateTimeKind.Utc), true, true, "Отсутствует необходимое сырьё" },
                    { new Guid("10000000-0000-0000-0000-000000000003"), null, new DateTime(2026, 9, 15, 0, 0, 0, 0, DateTimeKind.Utc), true, true, "Производственные мощности загружены" },
                    { new Guid("10000000-0000-0000-0000-000000000004"), null, new DateTime(2026, 9, 15, 0, 0, 0, 0, DateTimeKind.Utc), true, true, "Невозможно выполнить заказ в указанный срок" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_order_change_histories_rejection_reason_id",
                table: "order_change_histories",
                column: "rejection_reason_id");

            migrationBuilder.CreateIndex(
                name: "IX_rejection_reasons_company_id",
                table: "rejection_reasons",
                column: "company_id");

            migrationBuilder.Sql("""
                CREATE UNIQUE INDEX "UX_rejection_reasons_default_name_ci"
                ON rejection_reasons (lower(name))
                WHERE company_id IS NULL;
                """);

            migrationBuilder.Sql("""
                CREATE UNIQUE INDEX "UX_rejection_reasons_company_name_ci"
                ON rejection_reasons (company_id, lower(name))
                WHERE company_id IS NOT NULL;
                """);

            migrationBuilder.AddForeignKey(
                name: "FK_order_change_histories_rejection_reasons_rejection_reason_id",
                table: "order_change_histories",
                column: "rejection_reason_id",
                principalTable: "rejection_reasons",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_order_change_histories_rejection_reasons_rejection_reason_id",
                table: "order_change_histories");

            migrationBuilder.DropTable(
                name: "rejection_reasons");

            migrationBuilder.DropIndex(
                name: "IX_order_change_histories_rejection_reason_id",
                table: "order_change_histories");

            migrationBuilder.DropColumn(
                name: "rejection_reason_id",
                table: "order_change_histories");
        }
    }
}
