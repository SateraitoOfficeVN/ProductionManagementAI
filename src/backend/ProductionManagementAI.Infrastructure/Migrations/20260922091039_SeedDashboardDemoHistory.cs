using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductionManagementAI.Infrastructure.Migrations
{
    /// <summary>
    /// Demo history for the dashboard (DB-004 migration 3, WI-004 DEC-013/DEC-014). All dates are relative to the moment
    /// the migration runs, as WI-003's seed is (WI-003 DEC-011); everything else is constant. The plant timezone is a
    /// literal because a migration cannot read application options; it matches PlantOptions.TimeZone (WI-002 DEC-017).
    /// </summary>
    public partial class SeedDashboardDemoHistory : Migration
    {
        private const string PlantTimeZone = "Asia/Tokyo";

        /// <summary>Id prefix of WI-003's seeded rows (SeedDemoProductionOrders).</summary>
        private const string WiThreeSeedIdPrefix = "0197e4a0-0000-7000-8001-";

        /// <summary>Id prefix of the rows this migration inserts, used to delete exactly them on Down.</summary>
        private const string SeedIdPrefix = "0197e4a0-0000-7000-8002-";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Re-date WI-003's 16 completed demo orders over the last 33 days (DB-004 table "Re-dated existing
            //    completed orders"). Due dates are left alone, so Screen B's filters and overdue markers are unaffected.
            //    Only rows with WI-003's seed id that are still Completed are touched; elsewhere this does nothing.
            migrationBuilder.Sql(
                $"""
                UPDATE production_orders o
                SET completed_at_utc = now() - make_interval(days => r.completed_days_ago),
                    updated_at_utc   = now() - make_interval(days => r.completed_days_ago),
                    created_at_utc   = now() - make_interval(days => r.completed_days_ago + r.lead_days)
                FROM (VALUES
                    (74, 0, 9),
                    (43, 1, 12),
                    (24, 2, 15),
                    (49, 3, 11),
                    (80, 4, 18),
                    (18, 6, 8),
                    (37, 8, 14),
                    (12, 10, 21),
                    (73, 12, 10),
                    (31, 14, 16),
                    (67, 17, 13),
                    (6, 20, 19),
                    (61, 23, 9),
                    (55, 26, 22),
                    (30, 29, 12),
                    (68, 33, 17)
                ) AS r(seq, completed_days_ago, lead_days)
                WHERE o.id = ('{WiThreeSeedIdPrefix}' || lpad(r.seq::text, 12, '0'))::uuid
                  AND o.status = 'Completed';
                """);

            // 2. Insert 40 historical completed orders and 4 far-due active ones (DB-004 tables "New historical completed
            //    orders" and "New far-due active orders"). Guarded: only where WI-003's demo seed exists (a demo
            //    database) and none of these ids exist yet. Sequence numbers follow the seed year's counter, so orders
            //    a user created after WI-003's seed are never collided with.
            migrationBuilder.Sql(
                $"""
                INSERT INTO production_orders
                    (id, order_year, order_seq, product_id, quantity, due_date, status, notes,
                     created_at_utc, updated_at_utc, completed_at_utc)
                SELECT
                    ('{SeedIdPrefix}' || lpad(s.n::text, 12, '0'))::uuid,
                    plant.year,
                    base.last_seq + s.n,
                    p.id,
                    s.quantity,
                    CASE WHEN s.status = 'Completed'
                         THEN ((now() - make_interval(days => s.completed_days_ago)) AT TIME ZONE '{PlantTimeZone}')::date + s.due_offset
                         ELSE plant.today + s.due_offset END,
                    s.status,
                    NULL,
                    CASE WHEN s.status = 'Completed'
                         THEN now() - make_interval(days => s.completed_days_ago + s.lead_or_created_days)
                         ELSE now() - make_interval(days => s.lead_or_created_days) END,
                    CASE WHEN s.status = 'Completed'
                         THEN now() - make_interval(days => s.completed_days_ago)
                         ELSE now() - make_interval(days => s.lead_or_created_days) END,
                    CASE WHEN s.status = 'Completed'
                         THEN now() - make_interval(days => s.completed_days_ago) END
                FROM (VALUES
                    (1, 'Completed', 0, 11, 3, 320, 'P-1001'),
                    (2, 'Completed', 2, 14, -2, 540, 'P-1002'),
                    (3, 'Completed', 3, 9, 4, 150, 'P-1003'),
                    (4, 'Completed', 5, 16, 1, 880, 'P-1004'),
                    (5, 'Completed', 7, 12, 0, 95, 'P-1005'),
                    (6, 'Completed', 9, 20, -3, 1200, 'P-1006'),
                    (7, 'Completed', 11, 8, 5, 430, 'P-1007'),
                    (8, 'Completed', 13, 15, 2, 260, 'P-1008'),
                    (9, 'Completed', 15, 10, -1, 710, 'P-1009'),
                    (10, 'Completed', 16, 18, 6, 55, 'P-1010'),
                    (11, 'Completed', 19, 13, 3, 1900, 'P-1011'),
                    (12, 'Completed', 21, 9, 1, 340, 'P-1012'),
                    (13, 'Completed', 24, 22, -4, 620, 'P-1013'),
                    (14, 'Completed', 25, 11, 2, 180, 'P-1014'),
                    (15, 'Completed', 27, 14, 7, 2500, 'P-1015'),
                    (16, 'Completed', 30, 17, 0, 75, 'P-1016'),
                    (17, 'Completed', 32, 12, -2, 960, 'P-1017'),
                    (18, 'Completed', 35, 19, 4, 410, 'P-1018'),
                    (19, 'Completed', 37, 10, 1, 130, 'P-1019'),
                    (20, 'Completed', 40, 24, -5, 1450, 'P-1020'),
                    (21, 'Completed', 42, 13, 3, 290, 'P-1021'),
                    (22, 'Completed', 45, 16, 2, 820, 'P-1022'),
                    (23, 'Completed', 47, 11, -1, 65, 'P-1023'),
                    (24, 'Completed', 50, 20, 5, 1100, 'P-1024'),
                    (25, 'Completed', 53, 14, 1, 370, 'P-1025'),
                    (26, 'Completed', 56, 9, -3, 2200, 'P-1026'),
                    (27, 'Completed', 58, 18, 2, 145, 'P-1027'),
                    (28, 'Completed', 61, 12, 4, 690, 'P-1028'),
                    (29, 'Completed', 64, 21, -2, 310, 'P-1029'),
                    (30, 'Completed', 66, 15, 3, 1750, 'P-1030'),
                    (31, 'Completed', 69, 10, 1, 85, 'P-1001'),
                    (32, 'Completed', 71, 17, -6, 560, 'P-1002'),
                    (33, 'Completed', 74, 13, 2, 240, 'P-1003'),
                    (34, 'Completed', 76, 22, 5, 1300, 'P-1004'),
                    (35, 'Completed', 78, 11, 0, 420, 'P-1005'),
                    (36, 'Completed', 80, 16, -3, 95, 'P-1006'),
                    (37, 'Completed', 82, 14, 2, 780, 'P-1007'),
                    (38, 'Completed', 83, 19, 4, 210, 'P-1008'),
                    (39, 'Completed', 85, 12, -1, 1600, 'P-1009'),
                    (40, 'Completed', 88, 20, 3, 330, 'P-1010'),
                    (41, 'Draft', NULL, 5, 49, 600, 'P-1011'),
                    (42, 'Draft', NULL, 8, 56, 1800, 'P-1017'),
                    (43, 'InProgress', NULL, 12, 63, 450, 'P-1024'),
                    (44, 'Draft', NULL, 3, 42, 950, 'P-1003')
                ) AS s(n, status, completed_days_ago, lead_or_created_days, due_offset, quantity, sku)
                CROSS JOIN (
                    SELECT
                        (now() AT TIME ZONE '{PlantTimeZone}')::date AS today,
                        EXTRACT(YEAR FROM (now() AT TIME ZONE '{PlantTimeZone}'))::smallint AS year
                ) AS plant
                CROSS JOIN LATERAL (
                    SELECT COALESCE(max(c.last_seq), 0) AS last_seq
                    FROM production_order_number_counters c
                    WHERE c.order_year = plant.year
                ) AS base
                JOIN products p ON p.sku = s.sku
                WHERE EXISTS (SELECT 1 FROM production_orders WHERE id::text LIKE '{WiThreeSeedIdPrefix}%')
                  AND NOT EXISTS (SELECT 1 FROM production_orders WHERE id::text LIKE '{SeedIdPrefix}%');
                """);

            // 3. Advance the counter past the new rows, so Screen A continues after them (DB-002, DEC-013).
            migrationBuilder.Sql(
                $"""
                INSERT INTO production_order_number_counters (order_year, last_seq)
                SELECT o.order_year, max(o.order_seq)
                FROM production_orders o
                WHERE o.id::text LIKE '{SeedIdPrefix}%'
                GROUP BY o.order_year
                ON CONFLICT (order_year) DO UPDATE
                    SET last_seq = GREATEST(production_order_number_counters.last_seq, EXCLUDED.last_seq);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Removes the 44 inserted rows by their fixed ids. The counter stays advanced (no number is reused), and the
            // 16 re-dated rows keep their new timestamps — the recovery limit DB-004 records.
            migrationBuilder.Sql($"DELETE FROM production_orders WHERE id::text LIKE '{SeedIdPrefix}%';");
        }
    }
}
