using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentalPro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveItemTotalCostFromOrderItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "item_total_cost",
                table: "order_items");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "item_total_cost",
                table: "order_items",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }
    }
}
