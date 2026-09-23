using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductionManagementAI.Infrastructure.Migrations
{
    /// <summary>
    /// Automobile-parts demo data in Japanese (001_DB, WI-005 DEC-002/DEC-008). Renames the 30 seeded products by their
    /// fixed ids (codes unchanged) and rewrites the notes of WI-003's seeded orders. No schema change.
    /// </summary>
    public partial class LocalizeDemoDataToJapanese : Migration
    {
        /// <summary>Id prefix of WI-003's seeded rows (SeedDemoProductionOrders), the only rows that carry notes.</summary>
        private const string SeedIdPrefix = "0197e4a0-0000-7000-8001-";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Only seeded rows whose notes a user has not changed are rewritten; orders users created are never touched.
            migrationBuilder.Sql(
                $"""
                UPDATE production_orders
                SET notes = regexp_replace(notes, '^Demo order ([0-9]+)$', 'デモ用の製造指示 \1')
                WHERE id::text LIKE '{SeedIdPrefix}%' AND notes ~ '^Demo order [0-9]+$';
                """);

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "id",
                keyValue: new Guid("0197e4a0-0000-7000-8000-000000001001"),
                column: "name",
                value: "ブレーキキャリパー");

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "id",
                keyValue: new Guid("0197e4a0-0000-7000-8000-000000001002"),
                column: "name",
                value: "ブレーキディスクローター");

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "id",
                keyValue: new Guid("0197e4a0-0000-7000-8000-000000001003"),
                column: "name",
                value: "ブレーキパッド");

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "id",
                keyValue: new Guid("0197e4a0-0000-7000-8000-000000001004"),
                column: "name",
                value: "ドライブシャフト");

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "id",
                keyValue: new Guid("0197e4a0-0000-7000-8000-000000001005"),
                column: "name",
                value: "等速ジョイント");

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "id",
                keyValue: new Guid("0197e4a0-0000-7000-8000-000000001006"),
                column: "name",
                value: "トランスミッションケース");

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "id",
                keyValue: new Guid("0197e4a0-0000-7000-8000-000000001007"),
                column: "name",
                value: "ハブベアリング");

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "id",
                keyValue: new Guid("0197e4a0-0000-7000-8000-000000001008"),
                column: "name",
                value: "ウォーターポンプ");

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "id",
                keyValue: new Guid("0197e4a0-0000-7000-8000-000000001009"),
                column: "name",
                value: "エンジンマウント");

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "id",
                keyValue: new Guid("0197e4a0-0000-7000-8000-000000001010"),
                column: "name",
                value: "ラジエーター");

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "id",
                keyValue: new Guid("0197e4a0-0000-7000-8000-000000001011"),
                column: "name",
                value: "タイミングギア");

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "id",
                keyValue: new Guid("0197e4a0-0000-7000-8000-000000001012"),
                column: "name",
                value: "クラッチディスク");

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "id",
                keyValue: new Guid("0197e4a0-0000-7000-8000-000000001013"),
                column: "name",
                value: "ショックアブソーバー");

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "id",
                keyValue: new Guid("0197e4a0-0000-7000-8000-000000001014"),
                column: "name",
                value: "コイルスプリング");

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "id",
                keyValue: new Guid("0197e4a0-0000-7000-8000-000000001015"),
                column: "name",
                value: "エンジンワイヤーハーネス");

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "id",
                keyValue: new Guid("0197e4a0-0000-7000-8000-000000001016"),
                column: "name",
                value: "ボディワイヤーハーネス");

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "id",
                keyValue: new Guid("0197e4a0-0000-7000-8000-000000001017"),
                column: "name",
                value: "ヘッドランプユニット");

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "id",
                keyValue: new Guid("0197e4a0-0000-7000-8000-000000001018"),
                column: "name",
                value: "リレーボックス");

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "id",
                keyValue: new Guid("0197e4a0-0000-7000-8000-000000001019"),
                column: "name",
                value: "オルタネーター");

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "id",
                keyValue: new Guid("0197e4a0-0000-7000-8000-000000001020"),
                column: "name",
                value: "ECU ケース");

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "id",
                keyValue: new Guid("0197e4a0-0000-7000-8000-000000001021"),
                column: "name",
                value: "インタークーラー");

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "id",
                keyValue: new Guid("0197e4a0-0000-7000-8000-000000001022"),
                column: "name",
                value: "電動ファン");

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "id",
                keyValue: new Guid("0197e4a0-0000-7000-8000-000000001023"),
                column: "name",
                value: "オイルフィルター");

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "id",
                keyValue: new Guid("0197e4a0-0000-7000-8000-000000001024"),
                column: "name",
                value: "スロットルボディ");

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "id",
                keyValue: new Guid("0197e4a0-0000-7000-8000-000000001025"),
                column: "name",
                value: "ピストン");

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "id",
                keyValue: new Guid("0197e4a0-0000-7000-8000-000000001026"),
                column: "name",
                value: "コネクティングロッド");

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "id",
                keyValue: new Guid("0197e4a0-0000-7000-8000-000000001027"),
                column: "name",
                value: "サブフレーム");

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "id",
                keyValue: new Guid("0197e4a0-0000-7000-8000-000000001028"),
                column: "name",
                value: "ドアパネル");

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "id",
                keyValue: new Guid("0197e4a0-0000-7000-8000-000000001029"),
                column: "name",
                value: "ドアヒンジ");

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "id",
                keyValue: new Guid("0197e4a0-0000-7000-8000-000000001030"),
                column: "name",
                value: "ボルト・ナットキット");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                $"""
                UPDATE production_orders
                SET notes = regexp_replace(notes, '^デモ用の製造指示 ([0-9]+)$', 'Demo order \1')
                WHERE id::text LIKE '{SeedIdPrefix}%' AND notes ~ '^デモ用の製造指示 [0-9]+$';
                """);

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "id",
                keyValue: new Guid("0197e4a0-0000-7000-8000-000000001001"),
                column: "name",
                value: "Steel bracket");

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "id",
                keyValue: new Guid("0197e4a0-0000-7000-8000-000000001002"),
                column: "name",
                value: "Aluminium housing");

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "id",
                keyValue: new Guid("0197e4a0-0000-7000-8000-000000001003"),
                column: "name",
                value: "Control panel assembly");

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "id",
                keyValue: new Guid("0197e4a0-0000-7000-8000-000000001004"),
                column: "name",
                value: "Drive shaft");

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "id",
                keyValue: new Guid("0197e4a0-0000-7000-8000-000000001005"),
                column: "name",
                value: "Hydraulic valve");

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "id",
                keyValue: new Guid("0197e4a0-0000-7000-8000-000000001006"),
                column: "name",
                value: "Gearbox assembly");

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "id",
                keyValue: new Guid("0197e4a0-0000-7000-8000-000000001007"),
                column: "name",
                value: "Bearing housing");

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "id",
                keyValue: new Guid("0197e4a0-0000-7000-8000-000000001008"),
                column: "name",
                value: "Pump impeller");

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "id",
                keyValue: new Guid("0197e4a0-0000-7000-8000-000000001009"),
                column: "name",
                value: "Motor mount plate");

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "id",
                keyValue: new Guid("0197e4a0-0000-7000-8000-000000001010"),
                column: "name",
                value: "Conveyor roller");

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "id",
                keyValue: new Guid("0197e4a0-0000-7000-8000-000000001011"),
                column: "name",
                value: "Spur gear 40T");

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "id",
                keyValue: new Guid("0197e4a0-0000-7000-8000-000000001012"),
                column: "name",
                value: "Coupling flange");

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "id",
                keyValue: new Guid("0197e4a0-0000-7000-8000-000000001013"),
                column: "name",
                value: "Pneumatic cylinder");

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "id",
                keyValue: new Guid("0197e4a0-0000-7000-8000-000000001014"),
                column: "name",
                value: "Sensor bracket");

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "id",
                keyValue: new Guid("0197e4a0-0000-7000-8000-000000001015"),
                column: "name",
                value: "Cable harness A");

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "id",
                keyValue: new Guid("0197e4a0-0000-7000-8000-000000001016"),
                column: "name",
                value: "Cable harness B");

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "id",
                keyValue: new Guid("0197e4a0-0000-7000-8000-000000001017"),
                column: "name",
                value: "Terminal block unit");

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "id",
                keyValue: new Guid("0197e4a0-0000-7000-8000-000000001018"),
                column: "name",
                value: "Relay module");

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "id",
                keyValue: new Guid("0197e4a0-0000-7000-8000-000000001019"),
                column: "name",
                value: "Power supply unit");

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "id",
                keyValue: new Guid("0197e4a0-0000-7000-8000-000000001020"),
                column: "name",
                value: "PLC enclosure");

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "id",
                keyValue: new Guid("0197e4a0-0000-7000-8000-000000001021"),
                column: "name",
                value: "Heat sink");

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "id",
                keyValue: new Guid("0197e4a0-0000-7000-8000-000000001022"),
                column: "name",
                value: "Cooling fan assembly");

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "id",
                keyValue: new Guid("0197e4a0-0000-7000-8000-000000001023"),
                column: "name",
                value: "Filter housing");

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "id",
                keyValue: new Guid("0197e4a0-0000-7000-8000-000000001024"),
                column: "name",
                value: "Valve body");

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "id",
                keyValue: new Guid("0197e4a0-0000-7000-8000-000000001025"),
                column: "name",
                value: "Piston rod");

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "id",
                keyValue: new Guid("0197e4a0-0000-7000-8000-000000001026"),
                column: "name",
                value: "Spring assembly");

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "id",
                keyValue: new Guid("0197e4a0-0000-7000-8000-000000001027"),
                column: "name",
                value: "Welded frame");

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "id",
                keyValue: new Guid("0197e4a0-0000-7000-8000-000000001028"),
                column: "name",
                value: "Guard panel");

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "id",
                keyValue: new Guid("0197e4a0-0000-7000-8000-000000001029"),
                column: "name",
                value: "Hinge set");

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "id",
                keyValue: new Guid("0197e4a0-0000-7000-8000-000000001030"),
                column: "name",
                value: "Fastener kit");
        }
    }
}
