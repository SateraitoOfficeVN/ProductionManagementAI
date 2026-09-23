using System.Data.Common;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using ProductionManagementAI.Application.Dashboard;
using ProductionManagementAI.Application.Health;

namespace ProductionManagementAI.Application.Tests.Dashboard;

public class DashboardServiceTests
{
    [Fact]
    public async Task One_clock_reading_drives_today_the_window_and_asOf()
    {
        // 2026-09-21 15:30 UTC is 2026-09-22 00:30 in Tokyo: the plant's day has already turned.
        var now = new DateTimeOffset(2026, 9, 21, 15, 30, 0, TimeSpan.Zero);
        var time = new FakeTimeProvider(now);
        var reader = new CapturingReader();
        var service = new DashboardService(reader, new TokyoClock(time), time, NullLogger<DashboardService>.Instance);

        var response = await service.GetSnapshotAsync(CancellationToken.None);

        Assert.Equal(new DateOnly(2026, 9, 22), response.Today);
        Assert.Equal(new DateOnly(2026, 9, 22), reader.Window!.Today);
        Assert.Equal(now, response.AsOf);
        Assert.Equal("Asia/Tokyo", response.TimeZone);
    }

    private sealed class CapturingReader : IDashboardReader
    {
        public DashboardWindow? Window { get; private set; }

        public Task<DashboardRaw> ReadAsync(DashboardWindow window, CancellationToken cancellationToken)
        {
            Window = window;
            return Task.FromResult(new DashboardRaw([], [], [], [], [], new DeliveryRow(0, 0, 0, 0, 0, 0, null), []));
        }
    }
}

/// <summary>003_DD-FN §6 (TC-224, unit): a failed ping is a result, not an exception.</summary>
public class SystemHealthServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 22, 5, 0, 0, TimeSpan.Zero);

    private static SystemHealthService Service(IDatabasePing ping) =>
        new(ping, new FakeTimeProvider(Now), NullLogger<SystemHealthService>.Instance);

    [Fact]
    public async Task A_database_that_answers_is_ok()
    {
        var health = await Service(new Ping(_ => Task.CompletedTask)).CheckAsync(CancellationToken.None);

        Assert.Equal(new SystemHealthResponse("ok", Now), health);
    }

    [Fact]
    public async Task A_database_error_is_unavailable()
    {
        var health = await Service(new Ping(_ => throw new FakeDbException())).CheckAsync(CancellationToken.None);

        Assert.Equal("unavailable", health.Database);
    }

    [Fact]
    public async Task A_ping_that_outlasts_the_timeout_is_unavailable()
    {
        var started = DateTime.UtcNow;

        var health = await Service(new Ping(ct => Task.Delay(Timeout.Infinite, ct))).CheckAsync(CancellationToken.None);

        Assert.Equal("unavailable", health.Database);
        Assert.InRange(DateTime.UtcNow - started, SystemHealthService.PingTimeout, TimeSpan.FromSeconds(10));
    }

    [Fact]
    public async Task A_caller_that_goes_away_is_not_reported_as_unavailable()
    {
        using var cancelled = new CancellationTokenSource();
        await cancelled.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => Service(new Ping(ct => Task.Delay(Timeout.Infinite, ct))).CheckAsync(cancelled.Token));
    }

    private sealed class Ping(Func<CancellationToken, Task> body) : IDatabasePing
    {
        public Task PingAsync(CancellationToken cancellationToken) => body(cancellationToken);
    }

    private sealed class FakeDbException() : DbException("connection refused");
}
