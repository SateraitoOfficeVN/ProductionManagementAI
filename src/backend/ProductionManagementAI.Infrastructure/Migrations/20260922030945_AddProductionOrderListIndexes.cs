using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductionManagementAI.Infrastructure.Migrations
{
    /// <summary>
    /// Index support for the production-order list (DB-003): the default-sort / due-date-range btree, and the pg_trgm
    /// GIN index behind the order-number fragment filter (WI-003 DEC-010).
    ///
    /// Both indexes are created CONCURRENTLY, because production_orders is already in use (ai/rules/database.md).
    /// CONCURRENTLY cannot run inside a transaction, so those statements suppress it — which means this migration is
    /// NOT atomic. If it fails part-way, PostgreSQL leaves an INVALID index behind: find it with
    ///     SELECT indexrelid::regclass FROM pg_index WHERE NOT indisvalid;
    /// drop it with DROP INDEX CONCURRENTLY, then re-apply. IF NOT EXISTS makes that retry safe.
    /// </summary>
    public partial class AddProductionOrderListIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Transactional, and committed before the concurrent index builds below need it.
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,");

            migrationBuilder.Sql(
                """
                CREATE INDEX CONCURRENTLY IF NOT EXISTS ix_production_orders_due_date_order_number
                    ON production_orders (due_date, order_number);
                """,
                suppressTransaction: true);

            migrationBuilder.Sql(
                """
                CREATE INDEX CONCURRENTLY IF NOT EXISTS ix_production_orders_order_number_trgm
                    ON production_orders USING gin (order_number gin_trgm_ops);
                """,
                suppressTransaction: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reversing this only makes the list slower, never wrong. The extension stays: dropping it is unnecessary
            // and would break anything else that came to depend on it (DB-003 rollback plan).
            migrationBuilder.Sql(
                "DROP INDEX CONCURRENTLY IF EXISTS ix_production_orders_order_number_trgm;",
                suppressTransaction: true);

            migrationBuilder.Sql(
                "DROP INDEX CONCURRENTLY IF EXISTS ix_production_orders_due_date_order_number;",
                suppressTransaction: true);

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:pg_trgm", ",,");
        }
    }
}
