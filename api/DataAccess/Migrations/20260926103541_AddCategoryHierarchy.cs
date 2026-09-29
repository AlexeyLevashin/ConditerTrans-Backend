using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryHierarchy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:ltree", ",,");

            migrationBuilder.AddColumn<Guid>(
                name: "parent_id",
                table: "categories",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "path",
                table: "categories",
                type: "ltree",
                nullable: true);

            migrationBuilder.Sql("UPDATE categories SET path = text2ltree('c' || replace(id::text, '-', '')) WHERE path IS NULL;");

            migrationBuilder.AlterColumn<string>(
                name: "path",
                table: "categories",
                type: "ltree",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "ltree",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_categories_parent_id",
                table: "categories",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "IX_categories_path",
                table: "categories",
                column: "path")
                .Annotation("Npgsql:IndexMethod", "gist");

            migrationBuilder.AddForeignKey(
                name: "FK_categories_categories_parent_id",
                table: "categories",
                column: "parent_id",
                principalTable: "categories",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_categories_categories_parent_id",
                table: "categories");

            migrationBuilder.DropIndex(
                name: "IX_categories_parent_id",
                table: "categories");

            migrationBuilder.DropIndex(
                name: "IX_categories_path",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "parent_id",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "path",
                table: "categories");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:ltree", ",,");
        }
    }
}
