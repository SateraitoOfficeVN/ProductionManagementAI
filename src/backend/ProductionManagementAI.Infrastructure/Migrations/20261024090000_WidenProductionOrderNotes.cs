using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductionManagementAI.Infrastructure.Migrations
{
    /// <summary>
    /// WI-017 (REQ-090, REQ-092; 001_DB revision 2026-10-24): production-order notes up to 1000 code points.
    /// Widening a varchar is catalogue-only in PostgreSQL (no rewrite). Down fails while a note is longer than 500,
    /// so a rollback never truncates data (DEC-005).
    /// </summary>
    public partial class WidenProductionOrderNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("SET LOCAL lock_timeout = '5s'; SET LOCAL statement_timeout = '15s';");
            migrationBuilder.AlterColumn<string>(
                name: "notes", table: "production_orders", type: "character varying(1000)",
                maxLength: 1000, nullable: true, oldClrType: typeof(string),
                oldType: "character varying(500)", oldMaxLength: 500, oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("SET LOCAL lock_timeout = '5s'; SET LOCAL statement_timeout = '15s';");
            migrationBuilder.AlterColumn<string>(
                name: "notes", table: "production_orders", type: "character varying(500)",
                maxLength: 500, nullable: true, oldClrType: typeof(string),
                oldType: "character varying(1000)", oldMaxLength: 1000, oldNullable: true);
        }
    }
}
