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
                    (2, 5000, 29, 'Draft', NULL, 11, 10, 'P-1015'),
                    (3, 275, 20, 'Draft', NULL, 12, 10, 'P-1022'),
                    (4, 2060, 11, 'InProgress', NULL, 13, 10, 'P-1029'),
                    (5, 69, 2, 'InProgress', NULL, 14, 10, 'P-1006'),
                    (6, 206, 63, 'Completed', 'Demo order 6', 15, 10, 'P-1013'),
                    (7, 1715, 54, 'Cancelled', NULL, 16, 10, 'P-1020'),
                    (8, 480, 45, 'Draft', NULL, 17, 10, 'P-1027'),
                    (9, 137, 36, 'Draft', NULL, 18, 18, 'P-1004'),
                    (10, 1370, 27, 'InProgress', NULL, 19, 18, 'P-1011'),
                    (11, 411, 18, 'InProgress', 'Demo order 11', 20, 18, 'P-1018'),
                    (12, 68, 9, 'Completed', NULL, 21, 18, 'P-1025'),
                    (13, 1025, 70, 'Cancelled', NULL, 22, 18, 'P-1002'),
                    (14, 342, 61, 'Draft', NULL, 23, 18, 'P-1009'),
                    (15, 479, 52, 'Draft', NULL, 24, 18, 'P-1016'),
                    (16, 680, 43, 'InProgress', 'Demo order 16', 25, 18, 'P-1023'),
                    (17, 273, 34, 'InProgress', NULL, 26, 26, 'P-1030'),
                    (18, 410, 25, 'Completed', NULL, 27, 26, 'P-1007'),
                    (19, 335, 16, 'Cancelled', NULL, 28, 26, 'P-1014'),
                    (20, 204, 7, 'Draft', NULL, 29, 26, 'P-1021'),
                    (21, 341, 0, 'Draft', 'Demo order 21', 30, 26, 'P-1028'),
                    (22, 2390, -35, 'InProgress', NULL, 31, 26, 'P-1005'),
                    (23, 135, -29, 'InProgress', NULL, 32, 26, 'P-1012'),
                    (24, 272, -23, 'Completed', NULL, 33, 26, 'P-1019'),
                    (25, 2045, -17, 'Cancelled', NULL, 34, 34, 'P-1026'),
                    (26, 66, -11, 'Draft', 'Demo order 26', 35, 34, 'P-1003'),
                    (27, 203, -5, 'Draft', NULL, 36, 34, 'P-1010'),
                    (28, 1700, 35, 'Draft', NULL, 37, 34, 'P-1017'),
                    (29, 477, 26, 'InProgress', NULL, 38, 34, 'P-1024'),
                    (30, 134, 17, 'Completed', NULL, 39, 34, 'P-1001'),
                    (31, 1355, 8, 'Completed', 'Demo order 31', 40, 34, 'P-1008'),
                    (32, 408, 69, 'Draft', NULL, 41, 34, 'P-1015'),
                    (33, 65, 60, 'Draft', NULL, 42, 42, 'P-1022'),
                    (34, 1010, 51, 'Draft', NULL, 43, 42, 'P-1029'),
                    (35, 339, 42, 'InProgress', NULL, 44, 42, 'P-1006'),
                    (36, 476, 33, 'InProgress', 'Demo order 36', 45, 42, 'P-1013'),
                    (37, 665, 24, 'Completed', NULL, 46, 42, 'P-1020'),
                    (38, 270, 15, 'Draft', NULL, 47, 42, 'P-1027'),
                    (39, 407, 6, 'Draft', NULL, 48, 42, 'P-1004'),
                    (40, 320, 67, 'Draft', NULL, 49, 42, 'P-1011'),
                    (41, 201, 58, 'InProgress', 'Demo order 41', 10, 10, 'P-1018'),
                    (42, 338, 49, 'InProgress', NULL, 11, 10, 'P-1025'),
                    (43, 2375, 40, 'Completed', NULL, 12, 10, 'P-1002'),
                    (44, 132, 31, 'Cancelled', NULL, 13, 10, 'P-1009'),
                    (45, 269, 22, 'Draft', NULL, 14, 10, 'P-1016'),
                    (46, 2030, 13, 'Draft', 'Demo order 46', 15, 10, 'P-1023'),
                    (47, 63, 4, 'InProgress', NULL, 16, 10, 'P-1030'),
                    (48, 200, -39, 'InProgress', NULL, 17, 10, 'P-1007'),
                    (49, 1685, -33, 'Completed', NULL, 18, 18, 'P-1014'),
                    (50, 474, -27, 'Cancelled', NULL, 19, 18, 'P-1021'),
                    (51, 131, -21, 'Draft', 'Demo order 51', 20, 18, 'P-1028'),
                    (52, 1340, -15, 'Draft', NULL, 21, 18, 'P-1005'),
                    (53, 405, -9, 'InProgress', NULL, 22, 18, 'P-1012'),
                    (54, 62, -3, 'InProgress', NULL, 23, 18, 'P-1019'),
                    (55, 995, 32, 'Completed', NULL, 24, 18, 'P-1026'),
                    (56, 336, 23, 'Cancelled', 'Demo order 56', 25, 18, 'P-1003'),
                    (57, 473, 14, 'Draft', NULL, 26, 26, 'P-1010'),
                    (58, 650, 5, 'Draft', NULL, 27, 26, 'P-1017'),
                    (59, 267, 66, 'InProgress', NULL, 28, 26, 'P-1024'),
                    (60, 404, 57, 'InProgress', NULL, 29, 26, 'P-1001'),
                    (61, 305, 48, 'Completed', 'Demo order 61', 30, 26, 'P-1008'),
                    (62, 198, 39, 'Cancelled', NULL, 31, 26, 'P-1015'),
                    (63, 335, 30, 'Draft', NULL, 32, 26, 'P-1022'),
                    (64, 2360, 21, 'Draft', NULL, 33, 26, 'P-1029'),
                    (65, 129, 12, 'InProgress', NULL, 34, 34, 'P-1006'),
                    (66, 266, 3, 'InProgress', 'Demo order 66', 35, 34, 'P-1013'),
                    (67, 2015, 64, 'Completed', NULL, 36, 34, 'P-1020'),
                    (68, 60, 55, 'Completed', NULL, 37, 34, 'P-1027'),
                    (69, 197, 46, 'Draft', NULL, 38, 34, 'P-1004'),
                    (70, 1670, 37, 'Draft', NULL, 39, 34, 'P-1011'),
                    (71, 471, 28, 'Draft', 'Demo order 71', 40, 34, 'P-1018'),
                    (72, 128, 19, 'InProgress', NULL, 41, 34, 'P-1025'),
                    (73, 1325, 10, 'Completed', NULL, 42, 42, 'P-1002'),
                    (74, 402, 1, 'Completed', NULL, 43, 42, 'P-1009'),
                    (75, 59, -37, 'Draft', NULL, 44, 42, 'P-1016'),
                    (76, 980, -31, 'Draft', 'Demo order 76', 45, 42, 'P-1023'),
                    (77, 333, -25, 'Draft', NULL, 46, 42, 'P-1030'),
                    (78, 470, -19, 'InProgress', NULL, 47, 42, 'P-1007'),
                    (79, 635, -13, 'InProgress', NULL, 48, 42, 'P-1014'),
                    (80, 264, -7, 'Completed', NULL, 49, 42, 'P-1021')
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
