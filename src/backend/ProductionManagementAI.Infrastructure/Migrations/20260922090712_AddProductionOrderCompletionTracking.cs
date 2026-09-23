using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductionManagementAI.Infrastructure.Migrations
{
    /// <summary>
    /// Completion tracking (003_DB migration 1, WI-004 DEC-003/DEC-004): the column, the backfill of orders already
    /// completed, then the two checks — in one transaction, so a failing check rolls the backfill back too.
    /// The backend that sets completed_at_utc must be running before anyone completes an order on the migrated
    /// schema (003_DB "Deploy order").
    /// </summary>
    public partial class AddProductionOrderCompletionTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 003_DB migration 1. Expand step: a nullable column with no default is a catalog-only change.
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "completed_at_utc",
                table: "production_orders",
                type: "timestamp with time zone",
                nullable: true);

            // Backfill before the checks (WI-004 DEC-004): for a completed order, the last update is the completion
            // save or a later edit, never earlier than creation. Seeded demo orders are re-dated by
            // SeedDashboardDemoHistory.
            migrationBuilder.Sql(
                "UPDATE production_orders SET completed_at_utc = updated_at_utc WHERE status = 'Completed';");

            migrationBuilder.AddCheckConstraint(
                name: "ck_production_orders_completed_at_matches_status",
                table: "production_orders",
                sql: "(status = 'Completed') = (completed_at_utc IS NOT NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "ck_production_orders_completed_at_not_before_created",
                table: "production_orders",
                sql: "completed_at_utc IS NULL OR completed_at_utc >= created_at_utc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Recovery limit (003_DB): every completion time recorded after this migration is lost.
            migrationBuilder.DropCheckConstraint(
                name: "ck_production_orders_completed_at_matches_status",
                table: "production_orders");

            migrationBuilder.DropCheckConstraint(
                name: "ck_production_orders_completed_at_not_before_created",
                table: "production_orders");

            migrationBuilder.DropColumn(
                name: "completed_at_utc",
                table: "production_orders");
        }
    }
}
