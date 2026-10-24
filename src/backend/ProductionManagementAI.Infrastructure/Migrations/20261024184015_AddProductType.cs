using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductionManagementAI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 004_DB-TYPE: every existing row starts as FINISHED; the default exists only during this migration.
            migrationBuilder.AddColumn<string>(
                name: "product_type",
                table: "products",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "FINISHED");

            // DEC-005 demo mapping by immutable SKU; SKUs that do not exist are no-ops.
            migrationBuilder.Sql(
                """
                UPDATE products SET product_type = 'SEMI_FINISHED'
                WHERE sku IN ('P-1006', 'P-1020', 'P-1025', 'P-1026', 'P-1028');
                UPDATE products SET product_type = 'PART'
                WHERE sku IN ('P-1002', 'P-1003', 'P-1005', 'P-1007', 'P-1009', 'P-1011',
                              'P-1012', 'P-1014', 'P-1018', 'P-1023', 'P-1029', 'P-1030');
                """);

            migrationBuilder.AlterColumn<string>(
                name: "product_type",
                table: "products",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldDefaultValue: "FINISHED");

            migrationBuilder.AddCheckConstraint(
                name: "ck_products_product_type",
                table: "products",
                sql: "product_type IN ('FINISHED', 'SEMI_FINISHED', 'PART', 'RAW_MATERIAL')");

            migrationBuilder.Sql("GRANT UPDATE (product_type) ON products TO pmai_app;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("REVOKE UPDATE (product_type) ON products FROM pmai_app;");

            migrationBuilder.DropCheckConstraint(
                name: "ck_products_product_type",
                table: "products");

            migrationBuilder.DropColumn(
                name: "product_type",
                table: "products");
        }
    }
}
