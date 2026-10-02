using System.Data.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Time.Testing;
using ProductionManagementAI.Application.PlantCalendar;
using ProductionManagementAI.Infrastructure;

namespace ProductionManagementAI.Integration.Tests.PlantCalendar;

/// <summary>Provides one-shot transaction faults without changing production transaction behavior.</summary>
public sealed class CalendarTransactionFixture : IntegrationTestFixture
{
    /// <summary>Gets the controlled plant clock.</summary>
    public FakeTimeProvider Time { get; } = new(new DateTimeOffset(2031, 6, 11, 3, 0, 0, TimeSpan.Zero));
    /// <summary>Gets the commit acknowledgement fault hook.</summary>
    public CommitObserver Commits { get; } = new();
    /// <summary>Gets the conditional state update fault hook.</summary>
    public CommandObserver Commands { get; } = new();
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.ConfigureTestServices(services => {
            services.RemoveAll<TimeProvider>(); services.AddSingleton<TimeProvider>(Time);
            services.AddDbContext<AppDbContext>(o => o.AddInterceptors(Commits, Commands));
        });
    }
    /// <summary>Injects failure immediately before or after database COMMIT acknowledgement.</summary>
    public sealed class CommitObserver : DbTransactionInterceptor
    {
        /// <summary>Gets or sets the armed one-shot failure phase.</summary>
        public string? Fault { get; set; }
        /// <inheritdoc />
        public override ValueTask<InterceptionResult> TransactionCommittingAsync(DbTransaction transaction, TransactionEventData eventData, InterceptionResult result, CancellationToken cancellationToken = default)
        {
            if (Fault == "Before") { Fault = null; throw new TimeoutException("Injected commit transport loss"); }
            return ValueTask.FromResult(result);
        }
        /// <inheritdoc />
        public override Task TransactionCommittedAsync(DbTransaction transaction, TransactionEndEventData eventData, CancellationToken cancellationToken = default)
        {
            if (Fault == "After") { Fault = null; throw new TimeoutException("Injected acknowledgement loss"); }
            return Task.CompletedTask;
        }
    }
    /// <summary>Exercises atomic rollback when a required conditional update affects zero rows.</summary>
    public sealed class CommandObserver : DbCommandInterceptor
    {
        /// <summary>Gets or sets whether to suppress the next state update.</summary>
        public bool ZeroStateUpdate { get; set; }
        /// <inheritdoc />
        public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (ZeroStateUpdate && command.CommandText.StartsWith("UPDATE plant_calendar_state", StringComparison.Ordinal)) { ZeroStateUpdate = false; return ValueTask.FromResult(InterceptionResult<int>.SuppressWithResult(0)); }
            return ValueTask.FromResult(result);
        }
    }
}

/// <summary>Checks actual commit uncertainty and rollback atomicity against the restricted database login.</summary>
public sealed class CalendarTransactionTests(CalendarTransactionFixture fixture) : IClassFixture<CalendarTransactionFixture>, IAsyncLifetime
{
    public async Task InitializeAsync()
    {
        // Explicit owner fixture activation only; never startup/read activation.
        if (await fixture.ScalarAsOwnerAsync<long>("SELECT count(*) FROM plant_calendar_state") == 0)
            await fixture.ExecuteAsOwnerAsync("BEGIN; INSERT INTO plant_calendar_state (id,activated_on,time_zone_id,revision) VALUES (1,'2031-06-10','Asia/Tokyo',1); INSERT INTO plant_calendar_weekly_revisions (calendar_id,effective_from,working_weekdays,is_withdrawn,is_current,commit_revision) VALUES (1,'2031-06-10',31,false,true,1); COMMIT;");
    }
    public Task DisposeAsync() => Task.CompletedTask;

    [Theory]
    [InlineData("Before", 0L)]
    [InlineData("After", 1L)]
    public async Task CommitFaultIsUnknownWithNoReplayAndExactPersistedOutcome(string phase, long expectedRows)
    {
        using var scope = fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var repository = scope.ServiceProvider.GetRequiredService<ICalendarRepository>();
        var version = (await fixture.ScalarAsOwnerAsync<long>("SELECT revision FROM plant_calendar_state"));
        var date = phase == "Before" ? new DateOnly(2031, 6, 12) : new DateOnly(2031, 6, 13);
        db.Database.SetCommandTimeout(37); db.Database.AutoSavepointsEnabled = true;
        fixture.Commits.Fault = phase;
        var error = await Assert.ThrowsAsync<CalendarRuleException>(() => repository.WriteTransitionAsync(new("SaveException", date, version, null, IsWorking: false), CancellationToken.None));
        Assert.Equal("CALENDAR_WRITE_UNKNOWN", error.Failure.Code);
        Assert.Equal("Unknown", error.Failure.WriteOutcome);
        Assert.Equal(expectedRows, await fixture.ScalarAsOwnerAsync<long>($"SELECT count(*) FROM plant_calendar_exception_revisions WHERE calendar_date='{date:yyyy-MM-dd}'"));
        Assert.Equal(version + expectedRows, await fixture.ScalarAsOwnerAsync<long>("SELECT revision FROM plant_calendar_state"));
        Assert.Null(fixture.Commits.Fault);
        Assert.Equal(37, db.Database.GetCommandTimeout()); Assert.True(db.Database.AutoSavepointsEnabled);
        Assert.Null(db.Database.CurrentTransaction);
    }

    [Fact]
    public async Task ZeroConditionalStateUpdateRollsBackTheInsertedSnapshot()
    {
        using var scope = fixture.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ICalendarRepository>();
        var version = await fixture.ScalarAsOwnerAsync<long>("SELECT revision FROM plant_calendar_state");
        fixture.Commands.ZeroStateUpdate = true;
        var error = await Assert.ThrowsAsync<CalendarRuleException>(() => repository.WriteTransitionAsync(new("SaveException", new DateOnly(2031, 6, 14), version, null, IsWorking: false), CancellationToken.None));
        Assert.False(fixture.Commands.ZeroStateUpdate);
        Assert.Equal("CALENDAR_STALE", error.Failure.Code); Assert.Equal("NotApplied", error.Failure.WriteOutcome);
        Assert.Equal(0L, await fixture.ScalarAsOwnerAsync<long>("SELECT count(*) FROM plant_calendar_exception_revisions WHERE calendar_date='2031-06-14'"));
        Assert.Equal(version, await fixture.ScalarAsOwnerAsync<long>("SELECT revision FROM plant_calendar_state"));
    }
}
