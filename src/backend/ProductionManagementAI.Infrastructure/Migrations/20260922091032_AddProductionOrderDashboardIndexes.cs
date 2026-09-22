using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductionManagementAI.Infrastructure.Migrations
{
    /// <summary>
    /// Partial indexes for the dashboard (DB-004 migration 2): the active orders in due-date order, and recent
    /// completions. Created CONCURRENTLY because production_orders is in use, so this migration is NOT atomic: after a
    /// failure, find an INVALID index with
    ///     SELECT indexrelid::regclass FROM pg_index WHERE NOT indisvalid;
    /// drop it with DROP INDEX CONCURRENTLY, then re-apply. IF NOT EXISTS makes the retry safe.
    /// </summary>
    public partial class AddProductionOrderDashboardIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                CREATE INDEX CONCURRENTLY IF NOT EXISTS ix_production_orders_active_due_date
                    ON production_orders (due_date, order_number)
                    WHERE status IN ('Draft', 'InProgress');
                """,
                suppressTransaction: true);

            migrationBuilder.Sql(
                """
                CREATE INDEX CONCURRENTLY IF NOT EXISTS ix_production_orders_completed_at_utc
                    ON production_orders (completed_at_utc)
                    WHERE completed_at_utc IS NOT NULL;
                """,
                suppressTransaction: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverting only makes the dashboard slower, never wrong.
            migrationBuilder.Sql(
                "DROP INDEX CONCURRENTLY IF EXISTS ix_production_orders_completed_at_utc;",
                suppressTransaction: true);

            migrationBuilder.Sql(
                "DROP INDEX CONCURRENTLY IF EXISTS ix_production_orders_active_due_date;",
                suppressTransaction: true);
        }
    }
}
