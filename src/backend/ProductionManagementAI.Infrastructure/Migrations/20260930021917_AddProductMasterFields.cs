using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductionManagementAI.Infrastructure.Migrations;

/// <summary>Adds maintainable product fields while preserving existing product identities and order references.</summary>
public partial class AddProductMasterFields : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            DO $$
            BEGIN
                IF EXISTS (SELECT 1 FROM pg_index WHERE indexrelid = to_regclass('ux_products_sku_lower') AND NOT indisvalid) THEN
                    RAISE EXCEPTION 'An invalid WI-006 SKU index requires owner remediation before retry.';
                END IF;
                IF EXISTS (SELECT 1 FROM products GROUP BY lower(sku) HAVING count(*) > 1) THEN
                    RAISE EXCEPTION 'Duplicate case-folded product SKUs require remediation.';
                END IF;
                IF EXISTS (SELECT 1 FROM products WHERE sku <> btrim(sku) OR length(sku) = 0 OR length(btrim(name)) = 0) THEN
                    RAISE EXCEPTION 'Existing product fields violate WI-006 constraints.';
                END IF;
                IF (SELECT count(*) FROM products WHERE sku BETWEEN 'P-1001' AND 'P-1030') <> 30
                   OR EXISTS (
                       SELECT 1 FROM products
                       WHERE sku BETWEEN 'P-1001' AND 'P-1030'
                         AND id::text <> '0197e4a0-0000-7000-8000-00000000' || substring(sku from 3)
                   ) THEN
                    RAISE EXCEPTION 'The expected 30 demo product IDs/SKUs are not present.';
                END IF;
            END $$;
            """);

        migrationBuilder.AddColumn<string>(
            name: "drawing_number", table: "products", type: "character varying(100)",
            maxLength: 100, nullable: true);
        migrationBuilder.AddColumn<bool>(
            name: "is_active", table: "products", type: "boolean",
            nullable: false, defaultValue: true);
        migrationBuilder.AddColumn<string>(
            name: "unit", table: "products", type: "character varying(20)",
            maxLength: 20, nullable: true);
        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "updated_at_utc", table: "products", type: "timestamp with time zone",
            nullable: false, defaultValueSql: "now()");

        migrationBuilder.Sql(
            """
            UPDATE products AS p SET unit = v.unit
            FROM (VALUES
                ('P-1001', '個'), ('P-1002', '枚'), ('P-1003', '枚'),
                ('P-1004', '本'), ('P-1005', '個'), ('P-1006', '個'),
                ('P-1007', '個'), ('P-1008', '台'), ('P-1009', '個'),
                ('P-1010', '台'), ('P-1011', '個'), ('P-1012', '枚'),
                ('P-1013', '本'), ('P-1014', '本'), ('P-1015', 'セット'),
                ('P-1016', 'セット'), ('P-1017', '台'), ('P-1018', '個'),
                ('P-1019', '台'), ('P-1020', '個'), ('P-1021', '台'),
                ('P-1022', '台'), ('P-1023', '個'), ('P-1024', '個'),
                ('P-1025', '個'), ('P-1026', '本'), ('P-1027', '台'),
                ('P-1028', '枚'), ('P-1029', '個'), ('P-1030', 'セット')
            ) AS v(sku, unit)
            WHERE p.sku = v.sku AND p.id::text =
                '0197e4a0-0000-7000-8000-00000000' || substring(v.sku from 3);
            DO $$
            BEGIN
                IF EXISTS (SELECT 1 FROM products WHERE unit IS NULL) THEN
                    RAISE EXCEPTION 'Non-seed products need reviewed units before WI-006 migration.';
                END IF;
            END $$;
            """);

        migrationBuilder.AlterColumn<string>(
            name: "unit", table: "products", type: "character varying(20)",
            maxLength: 20, nullable: false, oldClrType: typeof(string),
            oldType: "character varying(20)", oldMaxLength: 20, oldNullable: true);
        migrationBuilder.AddCheckConstraint(
            name: "ck_products_name_not_blank", table: "products", sql: "length(btrim(name)) > 0");
        migrationBuilder.AddCheckConstraint(
            name: "ck_products_sku_trimmed", table: "products",
            sql: "sku = btrim(sku) AND length(sku) > 0");
        migrationBuilder.AddCheckConstraint(
            name: "ck_products_unit", table: "products",
            sql: "unit IN ('個', '本', '枚', '台', 'セット', 'kg', 'm')");

        migrationBuilder.Sql(
            """
            GRANT INSERT ON products TO pmai_app;
            GRANT UPDATE (name, unit, drawing_number, is_active, updated_at_utc) ON products TO pmai_app;
            """);

        // A failed concurrent build may leave an invalid index in pg_index; inspect it before retrying.
        migrationBuilder.Sql(
            "CREATE UNIQUE INDEX CONCURRENTLY IF NOT EXISTS ux_products_sku_lower ON products (lower(sku));",
            suppressTransaction: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder) =>
        throw new NotSupportedException(
            "Product master values and retirement history require a backup or forward fix; automatic Down is unsafe.");
}
