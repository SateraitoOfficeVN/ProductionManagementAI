using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ProductionManagementAI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductionOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "production_order_number_counters",
                columns: table => new
                {
                    order_year = table.Column<short>(type: "smallint", nullable: false),
                    last_seq = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_production_order_number_counters", x => x.order_year);
                    table.CheckConstraint("ck_production_order_number_counters_last_seq_range", "last_seq BETWEEN 1 AND 99999");
                });

            migrationBuilder.CreateTable(
                name: "products",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    sku = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_products", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "production_orders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    order_year = table.Column<short>(type: "smallint", nullable: false),
                    order_seq = table.Column<int>(type: "integer", nullable: false),
                    order_number = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false, computedColumnSql: "'PO-' || order_year::text || '-' || lpad(order_seq::text, 5, '0')", stored: true),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    due_date = table.Column<DateOnly>(type: "date", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Draft"),
                    notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_production_orders", x => x.id);
                    table.CheckConstraint("ck_production_orders_order_seq_range", "order_seq BETWEEN 1 AND 99999");
                    table.CheckConstraint("ck_production_orders_order_year_range", "order_year BETWEEN 2000 AND 9999");
                    table.CheckConstraint("ck_production_orders_quantity_positive", "quantity > 0");
                    table.CheckConstraint("ck_production_orders_status", "status IN ('Draft', 'InProgress', 'Completed', 'Cancelled')");
                    table.ForeignKey(
                        name: "fk_production_orders_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "products",
                columns: new[] { "id", "created_at_utc", "name", "sku" },
                values: new object[,]
                {
                    { new Guid("0197e4a0-0000-7000-8000-000000001001"), new DateTimeOffset(new DateTime(2026, 9, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Steel bracket", "P-1001" },
                    { new Guid("0197e4a0-0000-7000-8000-000000001002"), new DateTimeOffset(new DateTime(2026, 9, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Aluminium housing", "P-1002" },
                    { new Guid("0197e4a0-0000-7000-8000-000000001003"), new DateTimeOffset(new DateTime(2026, 9, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Control panel assembly", "P-1003" },
                    { new Guid("0197e4a0-0000-7000-8000-000000001004"), new DateTimeOffset(new DateTime(2026, 9, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Drive shaft", "P-1004" },
                    { new Guid("0197e4a0-0000-7000-8000-000000001005"), new DateTimeOffset(new DateTime(2026, 9, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Hydraulic valve", "P-1005" },
                    { new Guid("0197e4a0-0000-7000-8000-000000001006"), new DateTimeOffset(new DateTime(2026, 9, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Gearbox assembly", "P-1006" },
                    { new Guid("0197e4a0-0000-7000-8000-000000001007"), new DateTimeOffset(new DateTime(2026, 9, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Bearing housing", "P-1007" },
                    { new Guid("0197e4a0-0000-7000-8000-000000001008"), new DateTimeOffset(new DateTime(2026, 9, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Pump impeller", "P-1008" },
                    { new Guid("0197e4a0-0000-7000-8000-000000001009"), new DateTimeOffset(new DateTime(2026, 9, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Motor mount plate", "P-1009" },
                    { new Guid("0197e4a0-0000-7000-8000-000000001010"), new DateTimeOffset(new DateTime(2026, 9, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Conveyor roller", "P-1010" },
                    { new Guid("0197e4a0-0000-7000-8000-000000001011"), new DateTimeOffset(new DateTime(2026, 9, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Spur gear 40T", "P-1011" },
                    { new Guid("0197e4a0-0000-7000-8000-000000001012"), new DateTimeOffset(new DateTime(2026, 9, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Coupling flange", "P-1012" },
                    { new Guid("0197e4a0-0000-7000-8000-000000001013"), new DateTimeOffset(new DateTime(2026, 9, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Pneumatic cylinder", "P-1013" },
                    { new Guid("0197e4a0-0000-7000-8000-000000001014"), new DateTimeOffset(new DateTime(2026, 9, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Sensor bracket", "P-1014" },
                    { new Guid("0197e4a0-0000-7000-8000-000000001015"), new DateTimeOffset(new DateTime(2026, 9, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Cable harness A", "P-1015" },
                    { new Guid("0197e4a0-0000-7000-8000-000000001016"), new DateTimeOffset(new DateTime(2026, 9, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Cable harness B", "P-1016" },
                    { new Guid("0197e4a0-0000-7000-8000-000000001017"), new DateTimeOffset(new DateTime(2026, 9, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Terminal block unit", "P-1017" },
                    { new Guid("0197e4a0-0000-7000-8000-000000001018"), new DateTimeOffset(new DateTime(2026, 9, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Relay module", "P-1018" },
                    { new Guid("0197e4a0-0000-7000-8000-000000001019"), new DateTimeOffset(new DateTime(2026, 9, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Power supply unit", "P-1019" },
                    { new Guid("0197e4a0-0000-7000-8000-000000001020"), new DateTimeOffset(new DateTime(2026, 9, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "PLC enclosure", "P-1020" },
                    { new Guid("0197e4a0-0000-7000-8000-000000001021"), new DateTimeOffset(new DateTime(2026, 9, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Heat sink", "P-1021" },
                    { new Guid("0197e4a0-0000-7000-8000-000000001022"), new DateTimeOffset(new DateTime(2026, 9, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Cooling fan assembly", "P-1022" },
                    { new Guid("0197e4a0-0000-7000-8000-000000001023"), new DateTimeOffset(new DateTime(2026, 9, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Filter housing", "P-1023" },
                    { new Guid("0197e4a0-0000-7000-8000-000000001024"), new DateTimeOffset(new DateTime(2026, 9, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Valve body", "P-1024" },
                    { new Guid("0197e4a0-0000-7000-8000-000000001025"), new DateTimeOffset(new DateTime(2026, 9, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Piston rod", "P-1025" },
                    { new Guid("0197e4a0-0000-7000-8000-000000001026"), new DateTimeOffset(new DateTime(2026, 9, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Spring assembly", "P-1026" },
                    { new Guid("0197e4a0-0000-7000-8000-000000001027"), new DateTimeOffset(new DateTime(2026, 9, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Welded frame", "P-1027" },
                    { new Guid("0197e4a0-0000-7000-8000-000000001028"), new DateTimeOffset(new DateTime(2026, 9, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Guard panel", "P-1028" },
                    { new Guid("0197e4a0-0000-7000-8000-000000001029"), new DateTimeOffset(new DateTime(2026, 9, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Hinge set", "P-1029" },
                    { new Guid("0197e4a0-0000-7000-8000-000000001030"), new DateTimeOffset(new DateTime(2026, 9, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Fastener kit", "P-1030" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_production_orders_order_year_order_seq",
                table: "production_orders",
                columns: new[] { "order_year", "order_seq" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_production_orders_product_id",
                table: "production_orders",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_products_sku",
                table: "products",
                column: "sku",
                unique: true);

            // Least-privilege runtime login (001_DB "Application database privileges", DEC-016). The role is created
            // NOLOGIN here; its LOGIN PASSWORD is set outside the migration (deploy/db/init, test fixture) so no
            // secret lives in source. Migrations themselves run as the database owner.
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'pmai_app') THEN
                        CREATE ROLE pmai_app NOLOGIN;
                    END IF;
                    EXECUTE format('GRANT CONNECT ON DATABASE %I TO pmai_app', current_database());
                END
                $$;
                GRANT USAGE ON SCHEMA public TO pmai_app;
                GRANT SELECT ON products TO pmai_app;
                GRANT SELECT, INSERT, UPDATE ON production_orders, production_order_number_counters TO pmai_app;
                GRANT SELECT, INSERT, UPDATE ON users, roles, user_roles, user_claims, role_claims, user_logins, user_tokens TO pmai_app;
                GRANT DELETE ON user_roles, user_claims, user_logins, user_tokens TO pmai_app;
                GRANT USAGE, SELECT ON SEQUENCE user_claims_id_seq, role_claims_id_seq TO pmai_app;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // The role itself is kept: dropping it would fail if it owns or is granted anything elsewhere.
            migrationBuilder.Sql("""
                REVOKE ALL ON products, production_orders, production_order_number_counters FROM pmai_app;
                REVOKE ALL ON users, roles, user_roles, user_claims, role_claims, user_logins, user_tokens FROM pmai_app;
                REVOKE ALL ON SEQUENCE user_claims_id_seq, role_claims_id_seq FROM pmai_app;
                REVOKE USAGE ON SCHEMA public FROM pmai_app;
                """);

            migrationBuilder.DropTable(
                name: "production_order_number_counters");

            migrationBuilder.DropTable(
                name: "production_orders");

            migrationBuilder.DropTable(
                name: "products");
        }
    }
}
