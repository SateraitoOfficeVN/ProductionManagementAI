using System.Diagnostics;
using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;
using ProductionManagementAI.Domain.ProductionOrders;
using static ProductionManagementAI.Application.ProductionOrders.ProductionOrderTelemetry;

namespace ProductionManagementAI.Application.ProductionOrders;

/// <summary>
/// One projected row of the CSV export (002_DD-FN-CSV "Response / value mapping"). Separate from
/// <see cref="ProductionOrderListRow"/> so the list's projection stays unchanged (002_DB); this one adds notes and the
/// created/completed timestamps (WI-016 DEC-003).
/// </summary>
public sealed record ProductionOrderExportRow(
    string OrderNumber,
    string ProductSku,
    string ProductName,
    string ProductUnit,
    OrderLineResponse? Line,
    decimal Quantity,
    DateOnly DueDate,
    ProductionOrderStatus Status,
    string? Notes,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    DateTimeOffset? CompletedAtUtc);

/// <summary>
/// An export that passed every check and is ready to stream (002_DD-FN-CSV §1). It owns the read snapshot and the
/// activity until the response ends: the caller writes it with <see cref="ProductionOrderCsvWriter"/>, reports the
/// outcome with <see cref="Complete"/>, then disposes it.
/// </summary>
public sealed class ProductionOrderExport : IAsyncDisposable
{
    /// <summary>Row limit per export (WI-016 DEC-005). Over it the export is refused with MSG-E024.</summary>
    public const int MaxRows = 10_000;

    private readonly IAsyncDisposable? _snapshot;
    private readonly Activity? _activity;
    private readonly ILogger? _logger;
    private readonly string? _userId;
    private readonly long _startTimestamp;
    private bool _completed;

    public ProductionOrderExport(
        int count,
        DateTime plantNow,
        DateOnly plantToday,
        Func<DateTimeOffset, DateTime> toPlantTime,
        IAsyncEnumerable<ProductionOrderExportRow> rows,
        IAsyncDisposable? snapshot = null,
        Activity? activity = null,
        ILogger? logger = null,
        string? userId = null)
    {
        Count = count;
        PlantNow = plantNow;
        PlantToday = plantToday;
        ToPlantTime = toPlantTime;
        Rows = rows;
        _snapshot = snapshot;
        _activity = activity;
        _logger = logger;
        _userId = userId;
        _startTimestamp = Stopwatch.GetTimestamp();
    }

    /// <summary>Matching orders, counted in the same snapshot the rows are read from; sent as <c>X-Total-Count</c>.</summary>
    public int Count { get; }

    /// <summary>Plant-local time of the export, read once; used for the file name.</summary>
    public DateTime PlantNow { get; }

    /// <summary>Plant-local date of the export, read once; every row judges "overdue" against it.</summary>
    public DateOnly PlantToday { get; }

    public Func<DateTimeOffset, DateTime> ToPlantTime { get; }

    public IAsyncEnumerable<ProductionOrderExportRow> Rows { get; }

    /// <summary>Records the outcome once: counter, histogram, activity status and log 2005 (002_DD-FN-CSV §1, §4).</summary>
    public void Complete(string outcome, int rowsWritten, Exception? exception = null)
    {
        if (_completed)
        {
            return;
        }

        _completed = true;
        Exported.Add(1, new KeyValuePair<string, object?>("outcome", outcome));
        if (outcome == Outcomes.Success)
        {
            ExportRows.Record(rowsWritten);
        }

        _activity?.SetTag("outcome", outcome);
        _activity?.SetTag("result.rows", rowsWritten);
        if (outcome == Outcomes.Error)
        {
            _activity?.SetStatus(ActivityStatusCode.Error);
        }

        if (_logger is not null)
        {
            if (exception is not null)
            {
                ProductionOrderExportLog.ExportFailed(_logger, exception);
            }

            ProductionOrderExportLog.Exported(_logger, _userId, outcome, rowsWritten,
                (long)Stopwatch.GetElapsedTime(_startTimestamp).TotalMilliseconds);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_snapshot is not null)
        {
            await _snapshot.DisposeAsync();
        }

        _activity?.Dispose();
    }
}

/// <summary>
/// Writes API-PO-05's body (002_DD-API-CSV "Response body (CSV)", 002_DD-FN-CSV §3): UTF-8 with BOM, CRLF, RFC 4180
/// quoting, and a leading apostrophe on text that a spreadsheet would run as a formula (CWE-1236). Culture-invariant.
/// </summary>
public static class ProductionOrderCsvWriter
{
    public const string HeaderRow =
        "指示番号,製品コード,製品名,生産ライン,数量,単位,納期,ステータス,納期遅れ,備考,作成日時,更新日時,完了日時";

    private const string Overdue = "納期遅れ";
    private const string RetiredSuffix = " (使用停止)";
    private const int FlushEvery = 500;
    private static readonly byte[] Bom = [0xEF, 0xBB, 0xBF];
    private static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);

    /// <summary>Writes the whole file and returns the number of data rows written.</summary>
    public static async Task<int> WriteAsync(
        ProductionOrderExport export, Stream output, CancellationToken cancellationToken)
    {
        // The BOM is written explicitly: whether StreamWriter emits a preamble depends on the stream, and a second
        // BOM would show up as a stray character in the first header cell.
        await output.WriteAsync(Bom, cancellationToken);
        await using var writer = new StreamWriter(output, Utf8NoBom, bufferSize: 16 * 1024, leaveOpen: true)
        {
            NewLine = "\r\n",
        };

        await writer.WriteLineAsync(HeaderRow.AsMemory(), cancellationToken);
        var rows = 0;
        await foreach (var row in export.Rows.WithCancellation(cancellationToken))
        {
            await writer.WriteLineAsync(FormatRow(row, export.PlantToday, export.ToPlantTime).AsMemory(), cancellationToken);
            rows++;
            if (rows % FlushEvery == 0)
            {
                // Lets the browser receive data while the query is still running (002_DD-FN-CSV §3).
                await writer.FlushAsync(cancellationToken);
            }
        }

        await writer.FlushAsync(cancellationToken);
        return rows;
    }

    /// <summary>One CSV record without its line end (002_BD-CSV M-11–M-15).</summary>
    public static string FormatRow(
        ProductionOrderExportRow row, DateOnly plantToday, Func<DateTimeOffset, DateTime> toPlantTime)
    {
        string[] fields =
        [
            Field(row.OrderNumber, isText: true),
            Field(row.ProductSku, isText: true),
            Field(row.ProductName, isText: true),
            Field(FormatLine(row.Line), isText: true),
            Field(FormatQuantity(row.Quantity), isText: false),
            Field(row.ProductUnit, isText: true),
            Field(row.DueDate.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture), isText: false),
            Field(StatusLabel(row.Status), isText: true),
            Field(ProductionOrderListMapper.IsOverdue(row.DueDate, row.Status, plantToday) ? Overdue : null, isText: true),
            Field(row.Notes, isText: true),
            Field(FormatTimestamp(row.CreatedAtUtc, toPlantTime), isText: false),
            Field(FormatTimestamp(row.UpdatedAtUtc, toPlantTime), isText: false),
            Field(row.CompletedAtUtc is { } completed ? FormatTimestamp(completed, toPlantTime) : null, isText: false),
        ];
        return string.Join(',', fields);
    }

    /// <summary>
    /// One field: formula-like text gets a leading <c>'</c> (text columns only, never numbers or dates), then RFC 4180
    /// quoting when the value contains a comma, a double quote, CR or LF. Line breaks inside a value are kept
    /// (WI-016 DEC-013).
    /// </summary>
    public static string Field(string? value, bool isText)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        if (isText && value[0] is '=' or '+' or '-' or '@' or '\t' or '\r')
        {
            value = "'" + value;
        }

        return value.AsSpan().IndexOfAny(",\"\r\n") >= 0
            ? "\"" + value.Replace("\"", "\"\"", StringComparison.Ordinal) + "\""
            : value;
    }

    /// <summary>Scale is at most 3 (WI-006); no grouping, no trailing zeros, "." as the decimal point (M-12).</summary>
    public static string FormatQuantity(decimal quantity) => quantity.ToString("0.###", CultureInfo.InvariantCulture);

    private static string FormatTimestamp(DateTimeOffset utc, Func<DateTimeOffset, DateTime> toPlantTime) =>
        toPlantTime(utc).ToString("yyyy/MM/dd HH:mm", CultureInfo.InvariantCulture);

    private static string? FormatLine(OrderLineResponse? line) =>
        line is null ? null : $"{line.Code} — {line.Name}{(line.IsActive ? string.Empty : RetiredSuffix)}";

    /// <summary>The UI's status labels (002_BD M-05), fixed here because the file is read outside the app.</summary>
    public static string StatusLabel(ProductionOrderStatus status) => status switch
    {
        ProductionOrderStatus.Draft => "下書き",
        ProductionOrderStatus.InProgress => "進行中",
        ProductionOrderStatus.Completed => "完了",
        ProductionOrderStatus.Cancelled => "取消",
        _ => status.ToString(),
    };
}

/// <summary>Export log entries (002_DD-FN-CSV §1). Never order contents, notes or filter values.</summary>
public static partial class ProductionOrderExportLog
{
    [LoggerMessage(EventId = 2005, EventName = "ProductionOrdersExported", Level = LogLevel.Information,
        Message = "Production orders export by {UserId}: {Outcome}, {RowCount} rows in {DurationMs} ms")]
    public static partial void Exported(ILogger logger, string? userId, string outcome, int rowCount, long durationMs);

    [LoggerMessage(EventId = 2004, EventName = "ProductionOrderUnexpectedFailure", Level = LogLevel.Error,
        Message = "Production order export failed unexpectedly")]
    public static partial void ExportFailed(ILogger logger, Exception exception);
}
