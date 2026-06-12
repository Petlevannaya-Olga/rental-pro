using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentalPro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentFiscalizationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "fiscal_error_message",
                table: "payments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "fiscal_receipt_id",
                table: "payments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "fiscal_status",
                table: "payments",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "fiscalized_at",
                table: "payments",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "fiscal_error_message",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "fiscal_receipt_id",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "fiscal_status",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "fiscalized_at",
                table: "payments");
        }
    }
}
