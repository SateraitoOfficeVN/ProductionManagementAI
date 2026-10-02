using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductionManagementAI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ExpandPlantCalendar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("SET LOCAL lock_timeout = '5s'; SET LOCAL statement_timeout = '15s';");
            migrationBuilder.CreateTable(
                name: "plant_calendar_state",
                columns: table => new
                {
                    id = table.Column<short>(type: "smallint", nullable: false),
                    activated_on = table.Column<DateOnly>(type: "date", nullable: false),
                    time_zone_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    revision = table.Column<long>(type: "bigint", nullable: false, defaultValue: 1L),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_plant_calendar_state", x => x.id);
                    table.CheckConstraint("ck_calendar_state_date", "activated_on BETWEEN DATE '0001-01-01' AND DATE '9999-12-31'");
                    table.CheckConstraint("ck_calendar_state_revision", "revision >= 1");
                    table.CheckConstraint("ck_calendar_state_singleton", "id = 1");
                    table.CheckConstraint("ck_calendar_state_zone", "time_zone_id = btrim(time_zone_id) AND length(time_zone_id) > 0");
                });

            migrationBuilder.CreateTable(
                name: "plant_calendar_exception_revisions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    calendar_id = table.Column<short>(type: "smallint", nullable: false),
                    line_id = table.Column<Guid>(type: "uuid", nullable: true),
                    calendar_date = table.Column<DateOnly>(type: "date", nullable: false),
                    is_working = table.Column<bool>(type: "boolean", nullable: true),
                    working_hours = table.Column<decimal>(type: "numeric", nullable: true),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_removed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_current = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    commit_revision = table.Column<long>(type: "bigint", nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_calendar_exception_revisions", x => x.id);
                    table.CheckConstraint("ck_calendar_exception_date", "calendar_date BETWEEN DATE '0001-01-01' AND DATE '9999-12-31'");
                    table.CheckConstraint("ck_calendar_exception_hours", "working_hours IS NULL OR (working_hours > 0 AND working_hours <= 24 AND scale(working_hours) <= 3)");
                    table.CheckConstraint("ck_calendar_exception_payload", "(is_removed AND is_working IS NULL AND working_hours IS NULL AND reason IS NULL) OR (NOT is_removed AND is_working IS NOT NULL AND (is_working OR working_hours IS NULL))");
                    table.CheckConstraint("ck_calendar_exception_revision", "commit_revision >= 1");
                    table.ForeignKey(
                        name: "fk_calendar_exception_line",
                        column: x => x.line_id,
                        principalTable: "production_lines",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_calendar_exception_state",
                        column: x => x.calendar_id,
                        principalTable: "plant_calendar_state",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "plant_calendar_weekly_revisions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    calendar_id = table.Column<short>(type: "smallint", nullable: false),
                    effective_from = table.Column<DateOnly>(type: "date", nullable: false),
                    working_weekdays = table.Column<short>(type: "smallint", nullable: true),
                    is_withdrawn = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_current = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    commit_revision = table.Column<long>(type: "bigint", nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_calendar_weekly_revisions", x => x.id);
                    table.CheckConstraint("ck_calendar_weekly_date", "effective_from BETWEEN DATE '0001-01-01' AND DATE '9999-12-31'");
                    table.CheckConstraint("ck_calendar_weekly_payload", "(is_withdrawn AND working_weekdays IS NULL) OR (NOT is_withdrawn AND working_weekdays IS NOT NULL AND working_weekdays BETWEEN 0 AND 127)");
                    table.CheckConstraint("ck_calendar_weekly_revision", "commit_revision >= 1");
                    table.ForeignKey(
                        name: "fk_calendar_weekly_state",
                        column: x => x.calendar_id,
                        principalTable: "plant_calendar_state",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_calendar_exception_line",
                table: "plant_calendar_exception_revisions",
                column: "line_id",
                filter: "line_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ux_calendar_exception_commit",
                table: "plant_calendar_exception_revisions",
                columns: new[] { "calendar_id", "commit_revision" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_calendar_exception_current",
                table: "plant_calendar_exception_revisions",
                columns: new[] { "calendar_id", "line_id", "calendar_date" },
                unique: true,
                filter: "is_current")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "ux_calendar_weekly_commit",
                table: "plant_calendar_weekly_revisions",
                columns: new[] { "calendar_id", "commit_revision" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_calendar_weekly_current",
                table: "plant_calendar_weekly_revisions",
                columns: new[] { "calendar_id", "effective_from" },
                unique: true,
                descending: new[] { false, true },
                filter: "is_current");

            migrationBuilder.Sql("""
                REVOKE ALL ON public.plant_calendar_state, public.plant_calendar_weekly_revisions,
                    public.plant_calendar_exception_revisions FROM PUBLIC, pmai_app;
                GRANT SELECT ON public.plant_calendar_state, public.plant_calendar_weekly_revisions,
                    public.plant_calendar_exception_revisions TO pmai_app;
                GRANT UPDATE (revision, updated_at_utc) ON public.plant_calendar_state TO pmai_app;
                GRANT INSERT, UPDATE (is_current) ON public.plant_calendar_weekly_revisions,
                    public.plant_calendar_exception_revisions TO pmai_app;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("SET LOCAL lock_timeout = '5s'; SET LOCAL statement_timeout = '15s';");
            migrationBuilder.Sql("""
                DO $$ BEGIN
                    IF EXISTS (SELECT 1 FROM public.plant_calendar_state) THEN
                        RAISE EXCEPTION 'Activated calendar history cannot be dropped; use an approved forward repair.';
                    END IF;
                END $$;
                """);
            migrationBuilder.DropTable(
                name: "plant_calendar_exception_revisions");

            migrationBuilder.DropTable(
                name: "plant_calendar_weekly_revisions");

            migrationBuilder.DropTable(
                name: "plant_calendar_state");
        }
    }
}
