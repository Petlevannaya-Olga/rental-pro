using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentalPro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFilteredUniqueIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_users_email",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_users_login",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_users_phone_number",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_tools_article_number",
                table: "tools");

            migrationBuilder.DropIndex(
                name: "IX_tools_inventory_number",
                table: "tools");

            migrationBuilder.DropIndex(
                name: "IX_tools_serial_number",
                table: "tools");

            migrationBuilder.DropIndex(
                name: "IX_tool_statuses_name",
                table: "tool_statuses");

            migrationBuilder.DropIndex(
                name: "IX_tool_categories_name",
                table: "tool_categories");

            migrationBuilder.DropIndex(
                name: "IX_suppliers_name",
                table: "suppliers");

            migrationBuilder.DropIndex(
                name: "IX_roles_name",
                table: "roles");

            migrationBuilder.DropIndex(
                name: "IX_payment_types_name",
                table: "payment_types");

            migrationBuilder.DropIndex(
                name: "IX_payment_methods_name",
                table: "payment_methods");

            migrationBuilder.DropIndex(
                name: "IX_order_statuses_name",
                table: "order_statuses");

            migrationBuilder.DropIndex(
                name: "IX_manufacturers_name",
                table: "manufacturers");

            migrationBuilder.DropIndex(
                name: "IX_customers_email",
                table: "customers");

            migrationBuilder.DropIndex(
                name: "IX_customers_passport_series_passport_number",
                table: "customers");

            migrationBuilder.DropIndex(
                name: "IX_customers_phone_number",
                table: "customers");

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                table: "users",
                column: "email",
                unique: true,
                filter: "[deleted_at] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_users_login",
                table: "users",
                column: "login",
                unique: true,
                filter: "[deleted_at] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_users_phone_number",
                table: "users",
                column: "phone_number",
                unique: true,
                filter: "[deleted_at] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_tools_article_number",
                table: "tools",
                column: "article_number",
                unique: true,
                filter: "[deleted_at] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_tools_inventory_number",
                table: "tools",
                column: "inventory_number",
                unique: true,
                filter: "[deleted_at] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_tools_serial_number",
                table: "tools",
                column: "serial_number",
                unique: true,
                filter: "[deleted_at] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_tool_statuses_name",
                table: "tool_statuses",
                column: "name",
                unique: true,
                filter: "[deleted_at] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_tool_categories_name",
                table: "tool_categories",
                column: "name",
                unique: true,
                filter: "[deleted_at] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_suppliers_name",
                table: "suppliers",
                column: "name",
                unique: true,
                filter: "[deleted_at] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_roles_name",
                table: "roles",
                column: "name",
                unique: true,
                filter: "[deleted_at] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_payment_types_name",
                table: "payment_types",
                column: "name",
                unique: true,
                filter: "[deleted_at] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_payment_methods_name",
                table: "payment_methods",
                column: "name",
                unique: true,
                filter: "[deleted_at] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_order_statuses_name",
                table: "order_statuses",
                column: "name",
                unique: true,
                filter: "[deleted_at] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_manufacturers_name",
                table: "manufacturers",
                column: "name",
                unique: true,
                filter: "[deleted_at] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_customers_email",
                table: "customers",
                column: "email",
                unique: true,
                filter: "[deleted_at] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_customers_passport_series_passport_number",
                table: "customers",
                columns: new[] { "passport_series", "passport_number" },
                unique: true,
                filter: "[deleted_at] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_customers_phone_number",
                table: "customers",
                column: "phone_number",
                unique: true,
                filter: "[deleted_at] IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_users_email",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_users_login",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_users_phone_number",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_tools_article_number",
                table: "tools");

            migrationBuilder.DropIndex(
                name: "IX_tools_inventory_number",
                table: "tools");

            migrationBuilder.DropIndex(
                name: "IX_tools_serial_number",
                table: "tools");

            migrationBuilder.DropIndex(
                name: "IX_tool_statuses_name",
                table: "tool_statuses");

            migrationBuilder.DropIndex(
                name: "IX_tool_categories_name",
                table: "tool_categories");

            migrationBuilder.DropIndex(
                name: "IX_suppliers_name",
                table: "suppliers");

            migrationBuilder.DropIndex(
                name: "IX_roles_name",
                table: "roles");

            migrationBuilder.DropIndex(
                name: "IX_payment_types_name",
                table: "payment_types");

            migrationBuilder.DropIndex(
                name: "IX_payment_methods_name",
                table: "payment_methods");

            migrationBuilder.DropIndex(
                name: "IX_order_statuses_name",
                table: "order_statuses");

            migrationBuilder.DropIndex(
                name: "IX_manufacturers_name",
                table: "manufacturers");

            migrationBuilder.DropIndex(
                name: "IX_customers_email",
                table: "customers");

            migrationBuilder.DropIndex(
                name: "IX_customers_passport_series_passport_number",
                table: "customers");

            migrationBuilder.DropIndex(
                name: "IX_customers_phone_number",
                table: "customers");

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_login",
                table: "users",
                column: "login",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_phone_number",
                table: "users",
                column: "phone_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tools_article_number",
                table: "tools",
                column: "article_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tools_inventory_number",
                table: "tools",
                column: "inventory_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tools_serial_number",
                table: "tools",
                column: "serial_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tool_statuses_name",
                table: "tool_statuses",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tool_categories_name",
                table: "tool_categories",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_suppliers_name",
                table: "suppliers",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_roles_name",
                table: "roles",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payment_types_name",
                table: "payment_types",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payment_methods_name",
                table: "payment_methods",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_order_statuses_name",
                table: "order_statuses",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_manufacturers_name",
                table: "manufacturers",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_customers_email",
                table: "customers",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_customers_passport_series_passport_number",
                table: "customers",
                columns: new[] { "passport_series", "passport_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_customers_phone_number",
                table: "customers",
                column: "phone_number",
                unique: true);
        }
    }
}
