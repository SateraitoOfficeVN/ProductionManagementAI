using Microsoft.EntityFrameworkCore.Migrations;

namespace ProductionManagementAI.Infrastructure.Migrations;

/// <summary>Builds the existing-order assignment index concurrently and validates additive constraints.</summary>
public partial class IndexProductionLineAssignments : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // An interrupted concurrent build is not made safe by IF NOT EXISTS.
        migrationBuilder.Sql(
            """
            DO $guard$
            BEGIN
                IF EXISTS (
                    SELECT 1 FROM pg_catalog.pg_index i
                    WHERE i.indexrelid = pg_catalog.to_regclass('public.ix_orders_line_product')
                    AND (NOT i.indisvalid OR NOT i.indisready OR
                         pg_catalog.pg_get_indexdef(i.indexrelid) <>
                         'CREATE INDEX ix_orders_line_product ON public.production_orders USING btree (line_id, product_id) WHERE (line_id IS NOT NULL)')
                ) THEN
                    RAISE EXCEPTION 'The assignment index requires an owner-reviewed repair before migration retry.';
                END IF;
            END;
            $guard$;
            """, suppressTransaction: true);
        migrationBuilder.Sql(
            "CREATE INDEX CONCURRENTLY IF NOT EXISTS ix_orders_line_product ON public.production_orders (line_id, product_id) WHERE line_id IS NOT NULL;",
            suppressTransaction: true);
        migrationBuilder.Sql(
            """
            SET LOCAL lock_timeout = '5s';
            SET LOCAL statement_timeout = '10s';
            DO $verify$
            BEGIN
                IF NOT EXISTS (
                    SELECT 1 FROM pg_catalog.pg_index i
                    WHERE i.indexrelid = pg_catalog.to_regclass('public.ix_orders_line_product')
                    AND i.indisvalid AND i.indisready AND
                        pg_catalog.pg_get_indexdef(i.indexrelid) =
                        'CREATE INDEX ix_orders_line_product ON public.production_orders USING btree (line_id, product_id) WHERE (line_id IS NOT NULL)'
                ) THEN
                    RAISE EXCEPTION 'The assignment index does not match the approved valid definition.';
                END IF;
            END;
            $verify$;
            ALTER TABLE public.production_orders VALIDATE CONSTRAINT fk_orders_line_product;
            ALTER TABLE public.products VALIDATE CONSTRAINT ck_products_unit_revision;
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder) =>
        throw new NotSupportedException(
            "Production-line migrations require an owner-reviewed forward fix; automatic Down is unsafe.");
}
