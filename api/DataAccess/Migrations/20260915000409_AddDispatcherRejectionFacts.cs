using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddDispatcherRejectionFacts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "dispatcher_rejection_facts",
                columns: table => new
                {
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    rejected_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    rejection_reason_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dispatcher_rejection_facts", x => x.order_id);
                    table.ForeignKey(
                        name: "FK_dispatcher_rejection_facts_companies_company_id",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_dispatcher_rejection_facts_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_dispatcher_rejection_facts_rejection_reasons_rejection_reas~",
                        column: x => x.rejection_reason_id,
                        principalTable: "rejection_reasons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_dispatcher_rejection_facts_company_id_rejected_at",
                table: "dispatcher_rejection_facts",
                columns: new[] { "company_id", "rejected_at" });

            migrationBuilder.CreateIndex(
                name: "IX_dispatcher_rejection_facts_rejection_reason_id",
                table: "dispatcher_rejection_facts",
                column: "rejection_reason_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "dispatcher_rejection_facts");
        }
    }
}
