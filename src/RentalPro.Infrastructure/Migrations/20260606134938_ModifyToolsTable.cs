using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentalPro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ModifyToolsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tools_manufacturers_manufacturer_id",
                table: "tools");

            migrationBuilder.DropForeignKey(
                name: "FK_tools_tool_categories_category_id",
                table: "tools");

            migrationBuilder.DropForeignKey(
                name: "FK_tools_tool_statuses_status_id",
                table: "tools");

            migrationBuilder.AddForeignKey(
                name: "FK_tools_manufacturers_manufacturer_id",
                table: "tools",
                column: "manufacturer_id",
                principalTable: "manufacturers",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tools_tool_categories_category_id",
                table: "tools",
                column: "category_id",
                principalTable: "tool_categories",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tools_tool_statuses_status_id",
                table: "tools",
                column: "status_id",
                principalTable: "tool_statuses",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tools_manufacturers_manufacturer_id",
                table: "tools");

            migrationBuilder.DropForeignKey(
                name: "FK_tools_tool_categories_category_id",
                table: "tools");

            migrationBuilder.DropForeignKey(
                name: "FK_tools_tool_statuses_status_id",
                table: "tools");

            migrationBuilder.AddForeignKey(
                name: "FK_tools_manufacturers_manufacturer_id",
                table: "tools",
                column: "manufacturer_id",
                principalTable: "manufacturers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_tools_tool_categories_category_id",
                table: "tools",
                column: "category_id",
                principalTable: "tool_categories",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_tools_tool_statuses_status_id",
                table: "tools",
                column: "status_id",
                principalTable: "tool_statuses",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
