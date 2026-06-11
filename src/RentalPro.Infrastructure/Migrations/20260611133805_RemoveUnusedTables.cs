using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentalPro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnusedTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_order_items_orders_order_id",
                table: "order_items");

            migrationBuilder.DropForeignKey(
                name: "FK_order_items_tools_tool_id",
                table: "order_items");

            migrationBuilder.DropForeignKey(
                name: "FK_orders_customers_customer_id",
                table: "orders");

            migrationBuilder.DropForeignKey(
                name: "FK_orders_order_statuses_status_id",
                table: "orders");

            migrationBuilder.DropForeignKey(
                name: "FK_orders_users_user_id",
                table: "orders");

            migrationBuilder.DropForeignKey(
                name: "FK_payments_fines_fine_id",
                table: "payments");

            migrationBuilder.DropTable(
                name: "fines");

            migrationBuilder.DropTable(
                name: "maintenance_records");

            migrationBuilder.DropTable(
                name: "tool_purchases");

            migrationBuilder.DropTable(
                name: "maintenance_statuses");

            migrationBuilder.DropIndex(
                name: "IX_payments_fine_id",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "fine_id",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "deposit_total",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "total_cost",
                table: "orders");

            migrationBuilder.AddColumn<decimal>(
                name: "deposit_amount",
                table: "order_items",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddForeignKey(
                name: "FK_order_items_orders_order_id",
                table: "order_items",
                column: "order_id",
                principalTable: "orders",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_order_items_tools_tool_id",
                table: "order_items",
                column: "tool_id",
                principalTable: "tools",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_orders_customers_customer_id",
                table: "orders",
                column: "customer_id",
                principalTable: "customers",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_orders_order_statuses_status_id",
                table: "orders",
                column: "status_id",
                principalTable: "order_statuses",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_orders_users_user_id",
                table: "orders",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_order_items_orders_order_id",
                table: "order_items");

            migrationBuilder.DropForeignKey(
                name: "FK_order_items_tools_tool_id",
                table: "order_items");

            migrationBuilder.DropForeignKey(
                name: "FK_orders_customers_customer_id",
                table: "orders");

            migrationBuilder.DropForeignKey(
                name: "FK_orders_order_statuses_status_id",
                table: "orders");

            migrationBuilder.DropForeignKey(
                name: "FK_orders_users_user_id",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "deposit_amount",
                table: "order_items");

            migrationBuilder.AddColumn<Guid>(
                name: "fine_id",
                table: "payments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "deposit_total",
                table: "orders",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "total_cost",
                table: "orders",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "fines",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    fine_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    order_item_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fines", x => x.id);
                    table.ForeignKey(
                        name: "FK_fines_order_items_order_item_id",
                        column: x => x.order_item_id,
                        principalTable: "order_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "maintenance_statuses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintenance_statuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tool_purchases",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    purchase_date = table.Column<DateOnly>(type: "date", nullable: false),
                    supplier_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    tool_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tool_purchases", x => x.id);
                    table.ForeignKey(
                        name: "FK_tool_purchases_suppliers_supplier_id",
                        column: x => x.supplier_id,
                        principalTable: "suppliers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tool_purchases_tools_tool_id",
                        column: x => x.tool_id,
                        principalTable: "tools",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "maintenance_records",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    cost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    result = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    status_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    tool_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintenance_records", x => x.id);
                    table.ForeignKey(
                        name: "FK_maintenance_records_maintenance_statuses_status_id",
                        column: x => x.status_id,
                        principalTable: "maintenance_statuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_maintenance_records_tools_tool_id",
                        column: x => x.tool_id,
                        principalTable: "tools",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_maintenance_records_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_payments_fine_id",
                table: "payments",
                column: "fine_id");

            migrationBuilder.CreateIndex(
                name: "IX_fines_fine_date",
                table: "fines",
                column: "fine_date");

            migrationBuilder.CreateIndex(
                name: "IX_fines_order_item_id",
                table: "fines",
                column: "order_item_id");

            migrationBuilder.CreateIndex(
                name: "IX_maintenance_records_end_date",
                table: "maintenance_records",
                column: "end_date");

            migrationBuilder.CreateIndex(
                name: "IX_maintenance_records_start_date",
                table: "maintenance_records",
                column: "start_date");

            migrationBuilder.CreateIndex(
                name: "IX_maintenance_records_status_id",
                table: "maintenance_records",
                column: "status_id");

            migrationBuilder.CreateIndex(
                name: "IX_maintenance_records_tool_id",
                table: "maintenance_records",
                column: "tool_id");

            migrationBuilder.CreateIndex(
                name: "IX_maintenance_records_user_id",
                table: "maintenance_records",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_maintenance_statuses_name",
                table: "maintenance_statuses",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tool_purchases_purchase_date",
                table: "tool_purchases",
                column: "purchase_date");

            migrationBuilder.CreateIndex(
                name: "IX_tool_purchases_supplier_id",
                table: "tool_purchases",
                column: "supplier_id");

            migrationBuilder.CreateIndex(
                name: "IX_tool_purchases_tool_id",
                table: "tool_purchases",
                column: "tool_id");

            migrationBuilder.AddForeignKey(
                name: "FK_order_items_orders_order_id",
                table: "order_items",
                column: "order_id",
                principalTable: "orders",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_order_items_tools_tool_id",
                table: "order_items",
                column: "tool_id",
                principalTable: "tools",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_orders_customers_customer_id",
                table: "orders",
                column: "customer_id",
                principalTable: "customers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_orders_order_statuses_status_id",
                table: "orders",
                column: "status_id",
                principalTable: "order_statuses",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_orders_users_user_id",
                table: "orders",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_payments_fines_fine_id",
                table: "payments",
                column: "fine_id",
                principalTable: "fines",
                principalColumn: "id");
        }
    }
}
