using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductionManagementAI.Infrastructure.Migrations
{
    /// <summary>
    /// Demo production orders, so the list screen has something to page, sort and filter (DB-003, WI-003 DEC-007).
    ///
    /// Due dates and timestamps are computed from the plant-local date at migration time (DEC-011): fixed calendar
    /// dates would leave every seeded order overdue within months, and the screen would demo badly from then on.
    /// Everything else — ids, order numbers, products, quantities, statuses and the day offsets themselves — is a
    /// constant, so the data is deterministic given the run date. Tests therefore assert counts, statuses and
    /// offsets from today, never absolute dates.
    ///
    /// The insert is guarded by NOT EXISTS, so it only ever fills an empty table: re-applying it is a no-op, and a
    /// database that already holds real orders is untouched. EF Core's HasData cannot express any of this (it needs
    /// constant values), which is why this is raw SQL.
    ///
    /// The plant timezone appears here as a literal because a migration cannot read application options; it matches
    /// PlantOptions.TimeZone's configured value (WI-002 DEC-017).
    /// </summary>
    public partial class SeedDemoProductionOrders : Migration
    {
        private const string PlantTimeZone = "Asia/Tokyo";

        /// <summary>Id prefix of the seeded rows, used to delete exactly them on Down.</summary>
        private const string SeedIdPrefix = "0197e4a0-0000-7000-8001-";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                $"""
                INSERT INTO production_orders
                    (id, order_year, order_seq, product_id, quantity, due_date, status, notes,
                     created_at_utc, updated_at_utc)
                SELECT
                    ('{SeedIdPrefix}' || lpad(s.seq::text, 12, '0'))::uuid,
                    plant.year,
                    s.seq,
                    p.id,
                    s.quantity,
                    plant.today + s.due_offset,
                    s.status,
                    s.notes,
                    now() - make_interval(days => s.created_days_ago),
                    now() - make_interval(days => s.updated_days_ago)
                FROM (VALUES
                    (1, 1, -1, 'Draft', 'Demo order 1', 10, 10, 'P-1008'),
                    (2, 5000, 26, 'Draft', NULL, 11, 10, 'P-1015'),
                    (3, 275, 5, 'Draft', NULL, 12, 10, 'P-1022'),
                    (4, 2060, 24, 'InProgress', NULL, 13, 10, 'P-1029'),
                    (5, 69, 3, 'InProgress', NULL, 14, 10, 'P-1006'),
                    (6, 206, 22, 'Completed', 'Demo order 6', 15, 10, 'P-1013'),
                    (7, 1715, 1, 'Cancelled', NULL, 16, 10, 'P-1020'),
                    (8, 480, 20, 'Draft', NULL, 17, 10, 'P-1027'),
                    (9, 137, 39, 'Draft', NULL, 18, 18, 'P-1004'),
                    (10, 1370, 18, 'InProgress', NULL, 19, 18, 'P-1011'),
                    (11, 411, 37, 'InProgress', 'Demo order 11', 20, 18, 'P-1018'),
                    (12, 68, 16, 'Completed', NULL, 21, 18, 'P-1025'),
                    (13, 1025, 35, 'Cancelled', NULL, 22, 18, 'P-1002'),
                    (14, 342, 14, 'Draft', NULL, 23, 18, 'P-1009'),
                    (15, 479, 33, 'Draft', NULL, 24, 18, 'P-1016'),
                    (16, 680, 12, 'InProgress', 'Demo order 16', 25, 18, 'P-1023'),
                    (17, 273, 31, 'InProgress', NULL, 26, 26, 'P-1030'),
                    (18, 410, 10, 'Completed', NULL, 27, 26, 'P-1007'),
                    (19, 335, 29, 'Cancelled', NULL, 28, 26, 'P-1014'),
                    (20, 204, 8, 'Draft', NULL, 29, 26, 'P-1021'),
                    (21, 341, 0, 'Draft', 'Demo order 21', 30, 26, 'P-1028'),
                    (22, 2390, -10, 'InProgress', NULL, 31, 26, 'P-1005'),
                    (23, 135, -1, 'InProgress', NULL, 32, 26, 'P-1012'),
                    (24, 272, -6, 'Completed', NULL, 33, 26, 'P-1019'),
                    (25, 2045, -11, 'Cancelled', NULL, 34, 34, 'P-1026'),
                    (26, 66, -2, 'Draft', 'Demo order 26', 35, 34, 'P-1003'),
                    (27, 203, -7, 'Draft', NULL, 36, 34, 'P-1010'),
                    (28, 1700, 40, 'Draft', NULL, 37, 34, 'P-1017'),
                    (29, 477, 19, 'InProgress', NULL, 38, 34, 'P-1024'),
                    (30, 134, 38, 'Completed', NULL, 39, 34, 'P-1001'),
                    (31, 1355, 17, 'Completed', 'Demo order 31', 40, 34, 'P-1008'),
                    (32, 408, 36, 'Draft', NULL, 41, 34, 'P-1015'),
                    (33, 65, 15, 'Draft', NULL, 42, 42, 'P-1022'),
                    (34, 1010, 34, 'Draft', NULL, 43, 42, 'P-1029'),
                    (35, 339, 13, 'InProgress', NULL, 44, 42, 'P-1006'),
                    (36, 476, 32, 'InProgress', 'Demo order 36', 45, 42, 'P-1013'),
                    (37, 665, 11, 'Completed', NULL, 46, 42, 'P-1020'),
                    (38, 270, 30, 'Draft', NULL, 47, 42, 'P-1027'),
                    (39, 407, 9, 'Draft', NULL, 48, 42, 'P-1004'),
                    (40, 320, 28, 'Draft', NULL, 49, 42, 'P-1011'),
                    (41, 201, 7, 'InProgress', 'Demo order 41', 10, 10, 'P-1018'),
                    (42, 338, 26, 'InProgress', NULL, 11, 10, 'P-1025'),
                    (43, 2375, 5, 'Completed', NULL, 12, 10, 'P-1002'),
                    (44, 132, 24, 'Cancelled', NULL, 13, 10, 'P-1009'),
                    (45, 269, 3, 'Draft', NULL, 14, 10, 'P-1016'),
                    (46, 2030, 22, 'Draft', 'Demo order 46', 15, 10, 'P-1023'),
                    (47, 63, 1, 'InProgress', NULL, 16, 10, 'P-1030'),
                    (48, 200, -2, 'InProgress', NULL, 17, 10, 'P-1007'),
                    (49, 1685, -7, 'Completed', NULL, 18, 18, 'P-1014'),
                    (50, 474, -12, 'Cancelled', NULL, 19, 18, 'P-1021'),
                    (51, 131, -3, 'Draft', 'Demo order 51', 20, 18, 'P-1028'),
                    (52, 1340, -8, 'Draft', NULL, 21, 18, 'P-1005'),
                    (53, 405, -13, 'InProgress', NULL, 22, 18, 'P-1012'),
                    (54, 62, -4, 'InProgress', NULL, 23, 18, 'P-1019'),
                    (55, 995, 33, 'Completed', NULL, 24, 18, 'P-1026'),
                    (56, 336, 12, 'Cancelled', 'Demo order 56', 25, 18, 'P-1003'),
                    (57, 473, 31, 'Draft', NULL, 26, 26, 'P-1010'),
                    (58, 650, 10, 'Draft', NULL, 27, 26, 'P-1017'),
                    (59, 267, 29, 'InProgress', NULL, 28, 26, 'P-1024'),
                    (60, 404, 8, 'InProgress', NULL, 29, 26, 'P-1001'),
                    (61, 305, 27, 'Completed', 'Demo order 61', 30, 26, 'P-1008'),
                    (62, 198, 6, 'Cancelled', NULL, 31, 26, 'P-1015'),
                    (63, 335, 25, 'Draft', NULL, 32, 26, 'P-1022'),
                    (64, 2360, 4, 'Draft', NULL, 33, 26, 'P-1029'),
                    (65, 129, 23, 'InProgress', NULL, 34, 34, 'P-1006'),
                    (66, 266, 2, 'InProgress', 'Demo order 66', 35, 34, 'P-1013'),
                    (67, 2015, 21, 'Completed', NULL, 36, 34, 'P-1020'),
                    (68, 60, 40, 'Completed', NULL, 37, 34, 'P-1027'),
                    (69, 197, 19, 'Draft', NULL, 38, 34, 'P-1004'),
                    (70, 1670, 38, 'Draft', NULL, 39, 34, 'P-1011'),
                    (71, 471, 17, 'Draft', 'Demo order 71', 40, 34, 'P-1018'),
                    (72, 128, 36, 'InProgress', NULL, 41, 34, 'P-1025'),
                    (73, 1325, 15, 'Completed', NULL, 42, 42, 'P-1002'),
                    (74, 402, 0, 'Completed', NULL, 43, 42, 'P-1009'),
                    (75, 59, -13, 'Draft', NULL, 44, 42, 'P-1016'),
                    (76, 980, -4, 'Draft', 'Demo order 76', 45, 42, 'P-1023'),
                    (77, 333, -9, 'Draft', NULL, 46, 42, 'P-1030'),
                    (78, 470, -14, 'InProgress', NULL, 47, 42, 'P-1007'),
                    (79, 635, -5, 'InProgress', NULL, 48, 42, 'P-1014'),
                    (80, 264, -10, 'Completed', NULL, 49, 42, 'P-1021')
                ) AS s(seq, quantity, due_offset, status, notes, created_days_ago, updated_days_ago, sku)
                CROSS JOIN (
                    SELECT
                        (now() AT TIME ZONE '{PlantTimeZone}')::date AS today,
                        EXTRACT(YEAR FROM (now() AT TIME ZONE '{PlantTimeZone}'))::smallint AS year
                ) AS plant
                JOIN products p ON p.sku = s.sku
                WHERE NOT EXISTS (SELECT 1 FROM production_orders);
                """);

            // Keep the per-year counter consistent, so the next order created through Screen A continues the sequence
            // instead of colliding with a seeded order number (DB-002, DEC-013).
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
            // Deletes the seeded rows by their fixed ids, which also discards any edit a user made to one of them
            // through Screen A — the migration's only data-recovery limit (DB-003). Orders created by users have
            // different ids and are untouched, as is the counter, so their numbering keeps moving forward.
            migrationBuilder.Sql($"DELETE FROM production_orders WHERE id::text LIKE '{SeedIdPrefix}%';");
        }
    }
}
