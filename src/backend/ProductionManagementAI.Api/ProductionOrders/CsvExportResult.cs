using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using ProductionManagementAI.Application.ProductionOrders;
using static ProductionManagementAI.Application.ProductionOrders.ProductionOrderTelemetry;

namespace ProductionManagementAI.Api.ProductionOrders;

/// <summary>
/// Streams an export that passed every check (002_DD-FN-CSV §4). Headers go first (002_DD-API-CSV "Response headers"),
/// then the body. A failure after that cannot become Problem Details any more, so the connection is aborted: the
/// browser sees a network error and never a short file reported as a success.
/// </summary>
public sealed class CsvExportResult(ProductionOrderExport export) : IActionResult
{
    public const string TotalCountHeader = "X-Total-Count";

    public async Task ExecuteResultAsync(ActionContext context)
    {
        var http = context.HttpContext;
        var rows = 0;
        try
        {
            var stamp = export.PlantNow.ToString("yyyyMMdd-HHmm", CultureInfo.InvariantCulture);
            var disposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = $"production-orders_{stamp}.csv",
                FileNameStar = $"製造指示一覧_{stamp}.csv",
            };

            http.Response.StatusCode = StatusCodes.Status200OK;
            http.Response.ContentType = "text/csv; charset=utf-8";
            http.Response.Headers.ContentDisposition = disposition.ToString();
            http.Response.Headers[TotalCountHeader] = export.Count.ToString(CultureInfo.InvariantCulture);

            rows = await ProductionOrderCsvWriter.WriteAsync(export, http.Response.Body, http.RequestAborted);
            export.Complete(Outcomes.Success, rows);
        }
        catch (OperationCanceledException) when (http.RequestAborted.IsCancellationRequested)
        {
            // The user left the page or changed the view (002_DD-SPD-CSV P-17); nothing to send.
            export.Complete(Outcomes.Cancelled, rows);
        }
        catch (Exception ex)
        {
            export.Complete(Outcomes.Error, rows, ex);
            http.Abort();
        }
        finally
        {
            await export.DisposeAsync();
        }
    }
}
