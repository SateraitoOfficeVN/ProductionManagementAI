using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductionManagementAI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ExpandProductionLines : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("SET LOCAL lock_timeout = '5s'; SET LOCAL statement_timeout = '10s';");
            migrationBuilder.AddColumn<long>(
                name: "unit_revision",
                table: "products",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<Guid>(
                name: "line_id",
                table: "production_orders",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "production_lines",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    working_hours_per_day = table.Column<decimal>(type: "numeric", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_production_lines", x => x.id);
                    table.CheckConstraint("ck_production_lines_code_trimmed", "code = btrim(code) AND length(code) > 0");
                    table.CheckConstraint("ck_production_lines_hours_range", "working_hours_per_day > 0 AND working_hours_per_day <= 24");
                    table.CheckConstraint("ck_production_lines_hours_scale", "scale(working_hours_per_day) <= 3");
                    table.CheckConstraint("ck_production_lines_name_trimmed", "name = btrim(name) AND length(name) > 0");
                });

            migrationBuilder.CreateTable(
                name: "production_line_products",
                columns: table => new
                {
                    line_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    minutes_per_unit = table.Column<decimal>(type: "numeric", nullable: false),
                    confirmed_unit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    confirmed_unit_revision = table.Column<long>(type: "bigint", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_production_line_products", x => new { x.line_id, x.product_id });
                    table.CheckConstraint("ck_line_products_confirmed_revision", "confirmed_unit_revision >= 0");
                    table.CheckConstraint("ck_line_products_confirmed_unit", "confirmed_unit IN ('個','本','枚','台','セット','kg','m')");
                    table.CheckConstraint("ck_line_products_minutes_range", "minutes_per_unit > 0 AND minutes_per_unit <= 999999999.999");
                    table.CheckConstraint("ck_line_products_minutes_scale", "scale(minutes_per_unit) <= 3");
                    table.ForeignKey(
                        name: "fk_line_products_line",
                        column: x => x.line_id,
                        principalTable: "production_lines",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_line_products_product",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_line_products_product_line",
                table: "production_line_products",
                columns: new[] { "product_id", "line_id" });

            migrationBuilder.Sql(
                """
                ALTER TABLE public.products ADD CONSTRAINT ck_products_unit_revision
                    CHECK (unit_revision >= 0) NOT VALID;
                ALTER TABLE public.production_orders ADD CONSTRAINT fk_orders_line_product
                    FOREIGN KEY (line_id, product_id)
                    REFERENCES public.production_line_products (line_id, product_id)
                    MATCH SIMPLE ON UPDATE RESTRICT ON DELETE RESTRICT NOT VALID;
                CREATE UNIQUE INDEX ux_production_lines_code_lower ON public.production_lines (lower(code));
                CREATE FUNCTION public.advance_product_unit_revision() RETURNS trigger
                LANGUAGE plpgsql SECURITY INVOKER SET search_path = pg_catalog AS $function$
                BEGIN
                    IF NEW.unit IS DISTINCT FROM OLD.unit THEN
                        NEW.unit_revision := OLD.unit_revision + 1;
                    ELSE
                        NEW.unit_revision := OLD.unit_revision;
                    END IF;
                    RETURN NEW;
                END;
                $function$;
                CREATE TRIGGER tr_products_unit_revision BEFORE UPDATE OF unit ON public.products
                    FOR EACH ROW EXECUTE FUNCTION public.advance_product_unit_revision();
                GRANT SELECT, INSERT ON public.production_lines, public.production_line_products TO pmai_app;
                GRANT UPDATE (name, working_hours_per_day, is_active, updated_at_utc)
                    ON public.production_lines TO pmai_app;
                GRANT UPDATE (minutes_per_unit, confirmed_unit, confirmed_unit_revision, is_active, updated_at_utc)
                    ON public.production_line_products TO pmai_app;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) =>
            throw new NotSupportedException(
                "Production-line history and unit generations require a forward fix or authorized backup recovery; automatic Down is unsafe.");
    }
}
