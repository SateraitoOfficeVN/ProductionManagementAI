using System.Text;
using ProductionManagementAI.Application.ProductionOrders;
using ProductionManagementAI.Domain.ProductionOrders;

namespace ProductionManagementAI.Application.Tests.ProductionOrders;

/// <summary>002_DD-FN-CSV §3 / 002_DD-API-CSV "Response body (CSV)" — WI-016 TC-440 to TC-447.</summary>
public class ProductionOrderCsvWriterTests
{
    private static readonly DateOnly Today = new(2026, 10, 7);
    private static readonly TimeZoneInfo Tokyo = TimeZoneInfo.FindSystemTimeZoneById("Asia/Tokyo");

    private static DateTime ToTokyo(DateTimeOffset utc) => TimeZoneInfo.ConvertTime(utc, Tokyo).DateTime;

    private static ProductionOrderExportRow Row(
        string orderNumber = "PO-2026-00041",
        string name = "ブレーキキャリパー",
        decimal quantity = 120,
        string unit = "個",
        DateOnly? due = null,
        ProductionOrderStatus status = ProductionOrderStatus.InProgress,
        string? notes = null,
        OrderLineResponse? line = null,
        DateTimeOffset? completed = null) => new(
        orderNumber, "P-1001", name, unit, line, quantity, due ?? Today.AddDays(3), status, notes,
        new DateTimeOffset(2026, 9, 20, 1, 12, 0, TimeSpan.Zero),
        new DateTimeOffset(2026, 9, 28, 0, 15, 0, TimeSpan.Zero),
        completed);

    private static async Task<byte[]> WriteAsync(params ProductionOrderExportRow[] rows)
    {
        await using var export = new ProductionOrderExport(
            rows.Length, new DateTime(2026, 10, 7, 13, 45, 0), Today, ToTokyo, rows.ToAsyncEnumerable());
        using var body = new MemoryStream();
        var written = await ProductionOrderCsvWriter.WriteAsync(export, body, CancellationToken.None);
        Assert.Equal(rows.Length, written);
        return body.ToArray();
    }

    private static string Format(ProductionOrderExportRow row) => ProductionOrderCsvWriter.FormatRow(row, Today, ToTokyo);

    [Fact] // TC-440
    public async Task File_StartsWithOneBom_EndsEveryRecordWithCrlf_AndHasTheExactHeader()
    {
        var bytes = await WriteAsync(Row(), Row(orderNumber: "PO-2026-00042"));

        Assert.Equal(new byte[] { 0xEF, 0xBB, 0xBF }, bytes[..3]);
        Assert.NotEqual(new byte[] { 0xEF, 0xBB, 0xBF }, bytes[3..6]);
        var text = Encoding.UTF8.GetString(bytes[3..]);
        var records = text.Split("\r\n");
        Assert.Equal(4, records.Length); // header, 2 rows, and the empty string after the final CRLF
        Assert.Equal("", records[^1]);
        Assert.Equal(
            "指示番号,製品コード,製品名,生産ライン,数量,単位,納期,ステータス,納期遅れ,備考,作成日時,更新日時,完了日時",
            records[0]);
        Assert.DoesNotContain("\n", text.Replace("\r\n", string.Empty, StringComparison.Ordinal));
    }

    [Fact] // TC-440: an empty export is still a valid file with its header
    public async Task EmptyExport_IsBomAndHeaderOnly()
    {
        var text = Encoding.UTF8.GetString((await WriteAsync())[3..]);
        Assert.Equal(ProductionOrderCsvWriter.HeaderRow + "\r\n", text);
    }

    [Theory] // TC-441
    [InlineData("plain", "plain")]
    [InlineData("a,b", "\"a,b\"")]
    [InlineData("梱包は\"A\"仕様", "\"梱包は\"\"A\"\"仕様\"")]
    [InlineData("1行目\n2行目", "\"1行目\n2行目\"")]
    [InlineData("1行目\r\n2行目", "\"1行目\r\n2行目\"")]
    [InlineData("全角、カンマ", "全角、カンマ")]
    public void Field_QuotesOnlyWhenNeeded_AndDoublesQuotes(string value, string expected) =>
        Assert.Equal(expected, ProductionOrderCsvWriter.Field(value, isText: true));

    [Theory] // TC-442
    [InlineData("=1+1", "'=1+1")]
    [InlineData("+81-3", "'+81-3")]
    [InlineData("-5", "'-5")]
    [InlineData("@SUM(A1)", "'@SUM(A1)")]
    [InlineData("\tcmd", "'\tcmd")]
    [InlineData("=HYPERLINK(\"x\",\"y\")", "\"'=HYPERLINK(\"\"x\"\",\"\"y\"\")\"")]
    [InlineData("\rcmd", "\"'\rcmd\"")]
    [InlineData("a=b", "a=b")]
    public void Field_NeutralisesFormulaLikeText(string value, string expected) =>
        Assert.Equal(expected, ProductionOrderCsvWriter.Field(value, isText: true));

    [Fact] // TC-442 / TC-443: the prefix is for text only
    public void Field_NeverPrefixesNonTextColumns() =>
        Assert.Equal("-5", ProductionOrderCsvWriter.Field("-5", isText: false));

    [Theory] // TC-443
    [InlineData("0.125", "0.125")]
    [InlineData("12.500", "12.5")]
    [InlineData("120", "120")]
    [InlineData("999999999", "999999999")]
    public void Quantity_IsExact_InvariantAndUngrouped(string stored, string expected) =>
        Assert.Equal(expected, ProductionOrderCsvWriter.FormatQuantity(decimal.Parse(stored, System.Globalization.CultureInfo.InvariantCulture)));

    [Fact] // TC-443 / TC-444
    public void Row_FormatsDatesAndPlantTimestamps()
    {
        var row = Row(due: new DateOnly(2026, 10, 2)) with
        {
            CreatedAtUtc = new DateTimeOffset(2026, 10, 6, 15, 30, 0, TimeSpan.Zero),
            CompletedAtUtc = null,
        };

        var fields = Format(row).Split(',');

        Assert.Equal("2026/10/02", fields[6]);   // due date: plant-local date, not converted
        Assert.Equal("2026/10/07 00:30", fields[10]); // 15:30 UTC is 00:30 the next day in Tokyo
        Assert.Equal("2026/09/28 09:15", fields[11]);
        Assert.Equal("", fields[12]);            // not completed
    }

    [Fact] // TC-444
    public void Row_CompletedTimestampIsWrittenWhenSet()
    {
        var fields = Format(Row(status: ProductionOrderStatus.Completed,
            completed: new DateTimeOffset(2026, 10, 5, 8, 30, 0, TimeSpan.Zero))).Split(',');
        Assert.Equal("2026/10/05 17:30", fields[12]);
    }

    [Theory] // TC-445
    [InlineData(ProductionOrderStatus.Draft, -1, "下書き", "納期遅れ")]
    [InlineData(ProductionOrderStatus.InProgress, -1, "進行中", "納期遅れ")]
    [InlineData(ProductionOrderStatus.Completed, -1, "完了", "")]
    [InlineData(ProductionOrderStatus.Cancelled, -1, "取消", "")]
    [InlineData(ProductionOrderStatus.InProgress, 0, "進行中", "")]
    [InlineData(ProductionOrderStatus.Draft, 1, "下書き", "")]
    public void Row_StatusLabelAndOverdue(ProductionOrderStatus status, int dueOffsetDays, string label, string overdue)
    {
        var fields = Format(Row(status: status, due: Today.AddDays(dueOffsetDays))).Split(',');
        Assert.Equal(label, fields[7]);
        Assert.Equal(overdue, fields[8]);
    }

    [Fact] // TC-446
    public void Row_LineColumn()
    {
        var id = Guid.NewGuid();
        Assert.Equal("L-01 — 第1組立ライン", Format(Row(line: new(id, "L-01", "第1組立ライン", true))).Split(',')[3]);
        Assert.Equal("L-03 — 塗装ライン (使用停止)", Format(Row(line: new(id, "L-03", "塗装ライン", false))).Split(',')[3]);
        Assert.Equal("", Format(Row(line: null)).Split(',')[3]);
    }

    [Fact] // TC-447: maximum lengths, no spaces, survive a round trip through an RFC 4180 reader
    public async Task LongText_IsWrittenWhole_AndRoundTrips()
    {
        var longName = string.Concat(Enumerable.Repeat("ドライブシャフトASSY", 20))[..200];
        var longNotes = string.Concat(Enumerable.Repeat("長い備考,\"引用\"\r\n", 50))[..500];
        var bytes = await WriteAsync(Row(name: longName, notes: longNotes, unit: "kg", quantity: 12.5m));

        var records = ParseCsv(Encoding.UTF8.GetString(bytes[3..]));

        Assert.Equal(2, records.Count);
        Assert.All(records, r => Assert.Equal(13, r.Count));
        Assert.Equal(longName, records[1][2]);
        Assert.Equal(longNotes, records[1][9]);
        Assert.Equal("12.5", records[1][4]);
    }

    /// <summary>A minimal RFC 4180 reader, so the test does not trust the writer's own splitting.</summary>
    private static List<List<string>> ParseCsv(string text)
    {
        var records = new List<List<string>>();
        var record = new List<string>();
        var field = new StringBuilder();
        var quoted = false;
        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            if (quoted)
            {
                if (c == '"' && i + 1 < text.Length && text[i + 1] == '"') { field.Append('"'); i++; }
                else if (c == '"') { quoted = false; }
                else { field.Append(c); }
            }
            else if (c == '"') { quoted = true; }
            else if (c == ',') { record.Add(field.ToString()); field.Clear(); }
            else if (c == '\r' && i + 1 < text.Length && text[i + 1] == '\n')
            {
                record.Add(field.ToString()); field.Clear(); records.Add(record); record = []; i++;
            }
            else { field.Append(c); }
        }

        return records;
    }
}
