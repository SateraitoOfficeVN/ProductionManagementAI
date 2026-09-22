using System.Diagnostics;
using Microsoft.Extensions.Logging;
using ProductionManagementAI.Application.ProductionOrders;
using static ProductionManagementAI.Application.ProductionOrders.ProductionOrderTelemetry;

namespace ProductionManagementAI.Application.Dashboard;

/// <summary>DB-004 Q1–Q6 in one read-only snapshot (DD-003-FN §3; WI-004 DEC-015).</summary>
public interface IDashboardReader
{
    Task<DashboardRaw> ReadAsync(DashboardWindow window, CancellationToken cancellationToken);
}

/// <summary>
/// The dashboard snapshot (DD-003-FN §1). The clock is read once: T, every window, <c>asOf</c> and <c>today</c> all
/// derive from that single reading, so a dashboard computed across midnight cannot mix two "todays".
/// </summary>
public sealed partial class DashboardService(
    IDashboardReader reader,
    IPlantClock plantClock,
    TimeProvider timeProvider,
    ILogger<DashboardService> logger)
{
    public async Task<DashboardResponse> GetSnapshotAsync(CancellationToken cancellationToken)
    {
        using var activity = Source.StartActivity("ProductionOrder.Dashboard");
        try
        {
            var utcNow = timeProvider.GetUtcNow();
            var today = plantClock.DateOf(utcNow);
            var window = DashboardWindow.For(today, plantClock);
            var raw = await reader.ReadAsync(window, cancellationToken);
            var response = DashboardMapper.ToResponse(window, raw, utcNow);

            activity?.SetTag("dashboard.today", today.ToString("yyyy-MM-dd"));
            activity?.SetTag("dashboard.active_orders", response.StatusCounts.Draft + response.StatusCounts.InProgress);
            activity?.SetTag("dashboard.window_completed", response.OnTime.CompletedCount);
            DashboardLoaded.Add(1, new KeyValuePair<string, object?>("outcome", Outcomes.Success));
            return response;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            DashboardLoaded.Add(1, new KeyValuePair<string, object?>("outcome", Outcomes.Error));
            activity?.SetStatus(ActivityStatusCode.Error);
            LogSnapshotFailed(logger, ex);
            throw;
        }
    }

    [LoggerMessage(EventId = 3001, EventName = "DashboardSnapshotFailed", Level = LogLevel.Error,
        Message = "The dashboard snapshot failed.")]
    private static partial void LogSnapshotFailed(ILogger logger, Exception exception);
}
