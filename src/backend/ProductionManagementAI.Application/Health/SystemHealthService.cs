using System.Data.Common;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using static ProductionManagementAI.Application.ProductionOrders.ProductionOrderTelemetry;

namespace ProductionManagementAI.Application.Health;

/// <summary>A trivial database round trip (<c>SELECT 1</c>). A port, so tests can make it fail or stall (TC-224).</summary>
public interface IDatabasePing
{
    Task PingAsync(CancellationToken cancellationToken);
}

/// <param name="Database"><c>ok</c> or <c>unavailable</c> — the only component the server reports on (WI-004 DEC-020).</param>
public sealed record SystemHealthResponse(string Database, DateTimeOffset CheckedAt);

/// <summary>
/// Database reachability for the dashboard's health indicator (003_DD-FN §6). A failed ping is a result, not an
/// exception: the server answered, and saying the database did not is this endpoint's job.
/// </summary>
public sealed partial class SystemHealthService(IDatabasePing ping, TimeProvider timeProvider, ILogger<SystemHealthService> logger)
{
    /// <summary>The route; also the path the cookie handler never renews the session for (WI-004 DEC-019).</summary>
    public const string HealthPath = "/api/system/health";

    public static readonly TimeSpan PingTimeout = TimeSpan.FromSeconds(2);

    public const string Ok = "ok";
    public const string Unavailable = "unavailable";

    public async Task<SystemHealthResponse> CheckAsync(CancellationToken cancellationToken)
    {
        using var activity = Source.StartActivity("System.Health");
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(PingTimeout);

        string database;
        try
        {
            await ping.PingAsync(timeout.Token);
            database = Ok;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            database = Unavailable;
            Fail(activity, "Timeout");
        }
        catch (Exception ex) when (ex is DbException or InvalidOperationException or TimeoutException)
        {
            database = Unavailable;
            Fail(activity, ex.GetType().Name);
        }

        HealthChecks.Add(1, new KeyValuePair<string, object?>("database", database));
        return new SystemHealthResponse(database, timeProvider.GetUtcNow());
    }

    private void Fail(Activity? activity, string reason)
    {
        activity?.SetStatus(ActivityStatusCode.Error);

        // The type only: a connection error's message can carry host or credential fragments.
        LogPingFailed(logger, reason);
    }

    [LoggerMessage(EventId = 3101, EventName = "DatabasePingFailed", Level = LogLevel.Warning,
        Message = "The database health ping failed ({Reason}).")]
    private static partial void LogPingFailed(ILogger logger, string reason);
}
