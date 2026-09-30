using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductionManagementAI.Infrastructure.Migrations
{
    /// <summary>Converts old integer quantities to exact numeric values without changing existing values.</summary>
    public partial class ConvertOrderQuantityToNumeric : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM production_orders WHERE quantity <= 0 OR quantity > 999999999) THEN
                        RAISE EXCEPTION 'Existing order quantities violate the WI-006 range.';
                    END IF;
                    IF EXISTS (
                        SELECT 1 FROM production_orders o
                        LEFT JOIN products p ON p.id = o.product_id
                        WHERE p.id IS NULL OR p.unit IS NULL
                    ) THEN
                        RAISE EXCEPTION 'Existing orders require valid product units before quantity conversion.';
                    END IF;
                END $$;
                ALTER TABLE production_orders ALTER COLUMN quantity TYPE numeric USING quantity::numeric;
                """);

            migrationBuilder.AddCheckConstraint(
                name: "ck_production_orders_quantity_max",
                table: "production_orders",
                sql: "quantity <= 999999999");

            migrationBuilder.AddCheckConstraint(
                name: "ck_production_orders_quantity_scale",
                table: "production_orders",
                sql: "scale(quantity) <= 3");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            throw new NotSupportedException(
                "A fractional production order cannot be converted to integer without data loss; restore a backup or forward-fix.");
        }
    }
}
