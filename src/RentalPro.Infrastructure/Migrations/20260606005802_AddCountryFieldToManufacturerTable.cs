using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentalPro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCountryFieldToManufacturerTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "description",
                table: "manufacturers");

            migrationBuilder.AddColumn<string>(
                name: "country",
                table: "manufacturers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "country",
                table: "manufacturers");

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "manufacturers",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }
    }
}
