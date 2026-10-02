using System.Data.Common;
using System.Net;
using System.Net.Http.Json;
using static ProductionManagementAI.Integration.Tests.ProductionOrders.ProductionOrderApi;
using Npgsql;
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

/// <summary>Controls one interleaving after a real database reader establishes visibility.</summary>
public sealed class CalendarSnapshotFixture : IntegrationTestFixture
{
    /// <summary>Gets the deterministic plant clock.</summary>
    public FakeTimeProvider Time { get; } = new(new DateTimeOffset(2031, 6, 11, 3, 0, 0, TimeSpan.Zero));
    /// <summary>Gets the single-use read interleaving hook.</summary>
    public ReaderObserver Observer { get; } = new();
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.ConfigureTestServices(services => {
            services.RemoveAll<TimeProvider>();services.AddSingleton<TimeProvider>(Time);
            services.AddDbContext<AppDbContext>(o => o.AddInterceptors(Observer));
        });
    }
    /// <summary>Interleaves an independent commit or clock change once after matching SQL executes.</summary>
    public sealed class ReaderObserver : DbCommandInterceptor
    {
        /// <summary>Gets or sets the matching SQL fragment.</summary>
        public string? Fragment { get; set; }
        /// <summary>Gets or sets the independent action.</summary>
        public Func<Task>? AfterRead { get; set; }
        /// <inheritdoc />
        public override async ValueTask<DbDataReader> ReaderExecutedAsync(DbCommand command, CommandExecutedEventData eventData, DbDataReader result, CancellationToken cancellationToken = default)
        {
            if (AfterRead is { } action && Fragment is { } fragment && command.CommandText.Contains(fragment, StringComparison.Ordinal)) { AfterRead = null;await action(); }
            return result;
        }
    }
}

/// <summary>Checks coherent read snapshots, fresh post-lock dates and global unrelated-write conflicts.</summary>
public sealed class CalendarSnapshotTests(CalendarSnapshotFixture fixture) : IClassFixture<CalendarSnapshotFixture>,IAsyncLifetime
{
    public async Task InitializeAsync()
    {
        fixture.Time.AdjustTime(new DateTimeOffset(2031,6,11,3,0,0,TimeSpan.Zero));
        if (await fixture.ScalarAsOwnerAsync<long>("SELECT count(*) FROM plant_calendar_state") == 0)
            await fixture.ExecuteAsOwnerAsync("BEGIN; INSERT INTO plant_calendar_state (id,activated_on,time_zone_id,revision) VALUES (1,'2031-06-10','Asia/Tokyo',1); INSERT INTO plant_calendar_weekly_revisions (calendar_id,effective_from,working_weekdays,is_withdrawn,is_current,commit_revision) VALUES (1,'2031-06-10',31,false,true,1); COMMIT;");
    }
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task MonthKeepsStateAndRuleHeadsInOneSnapshotAcrossIndependentCommit()
    {
        var before = await fixture.ScalarAsOwnerAsync<long>("SELECT revision FROM plant_calendar_state");
        fixture.Observer.Fragment = "FROM plant_calendar_state";
        fixture.Observer.AfterRead = () => fixture.ExecuteAsOwnerAsync($"BEGIN; INSERT INTO plant_calendar_weekly_revisions (calendar_id,effective_from,working_weekdays,is_withdrawn,is_current,commit_revision) VALUES (1,'2031-06-11',0,false,true,{before+1}); UPDATE plant_calendar_state SET revision={before+1}; COMMIT;");
        using var scope = fixture.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<PlantCalendarService>();
        var old = await service.MonthAsync(new(Date:new DateOnly(2031,6,1)),CancellationToken.None);
        Assert.Null(old.Failure);Assert.NotNull(old.Value);Assert.Null(fixture.Observer.AfterRead);
        Assert.Equal(before.ToString(System.Globalization.CultureInfo.InvariantCulture),old.Value.Context.Version);
        Assert.Equal("Working",old.Value.Days.Single(x => x.Date == new DateOnly(2031,6,11)).State);
        var fresh = await service.MonthAsync(new(Date:new DateOnly(2031,6,1)),CancellationToken.None);
        Assert.Null(fresh.Failure);Assert.NotNull(fresh.Value);
        Assert.Equal((before+1).ToString(System.Globalization.CultureInfo.InvariantCulture),fresh.Value.Context.Version);
        Assert.Equal("Closed",fresh.Value.Days.Single(x => x.Date == new DateOnly(2031,6,11)).State);
    }

    [Fact]
    public async Task DateValidationUsesFreshClockAfterTargetQuery()
    {
        using var scope=fixture.Services.CreateScope();
        var repository=scope.ServiceProvider.GetRequiredService<ICalendarRepository>();
        var before=await fixture.ScalarAsOwnerAsync<long>("SELECT revision FROM plant_calendar_state");
        fixture.Observer.Fragment="FROM plant_calendar_exception_revisions";
        fixture.Observer.AfterRead=()=>{fixture.Time.Advance(TimeSpan.FromDays(1));return Task.CompletedTask;};
        var error=await Assert.ThrowsAsync<CalendarRuleException>(()=>repository.WriteTransitionAsync(new("SaveException",new DateOnly(2031,6,11),before,null,IsWorking:false),CancellationToken.None));
        Assert.Null(fixture.Observer.AfterRead);Assert.Equal("CALENDAR_PAST_DATE",error.Failure.Code);Assert.Equal("NotApplied",error.Failure.WriteOutcome);
        Assert.Equal(before,await fixture.ScalarAsOwnerAsync<long>("SELECT revision FROM plant_calendar_state"));
        Assert.Equal(0L,await fixture.ScalarAsOwnerAsync<long>("SELECT count(*) FROM plant_calendar_exception_revisions WHERE calendar_date='2031-06-11'"));
    }

    [Fact]
    public async Task LineShareBlocksNonKeyRetirementAndRetiredRemovalRemainsPermitted()
    {
        const string lineId = "33333333-3333-3333-3333-333333333333";
        await fixture.ExecuteAsOwnerAsync("INSERT INTO production_lines (id,code,name,working_hours_per_day,is_active,created_at_utc,updated_at_utc) VALUES ('33333333-3333-3333-3333-333333333333','CAL-LOCK','lock fixture',8,true,now(),now())");
        var before=await fixture.ScalarAsOwnerAsync<long>("SELECT revision FROM plant_calendar_state");
        fixture.Observer.Fragment="FROM public.production_lines";
        fixture.Observer.AfterRead=async()=>{
            var blocked=await Assert.ThrowsAsync<PostgresException>(()=>fixture.ExecuteAsOwnerAsync("SET lock_timeout='250ms'; UPDATE production_lines SET is_active=false WHERE id='33333333-3333-3333-3333-333333333333'"));
            Assert.Equal("55P03",blocked.SqlState);
        };
        using var scope=fixture.Services.CreateScope();
        var service=scope.ServiceProvider.GetRequiredService<PlantCalendarService>();
        var save=await service.SaveExceptionAsync(new("SaveException",new DateOnly(2031,6,20),before,null,Guid.Parse(lineId),IsWorking:false),CancellationToken.None);
        Assert.Null(save.Failure);Assert.NotNull(save.Value);Assert.Null(fixture.Observer.AfterRead);
        var target=Assert.IsType<ExceptionSnapshot>(save.Value.Target);
        await fixture.ExecuteAsOwnerAsync("UPDATE production_lines SET is_active=false WHERE id='33333333-3333-3333-3333-333333333333'");
        var unchangedRetired=await service.SaveExceptionAsync(new("SaveException",new DateOnly(2031,6,20),before+1,target.Id,Guid.Parse(lineId),IsWorking:false),CancellationToken.None);
        Assert.Equal("CALENDAR_LINE_RETIRED",unchangedRetired.Failure?.Code);
        var remove=await service.RemoveExceptionAsync(new("RemoveException",new DateOnly(2031,6,20),before+1,target.Id,Guid.Parse(lineId)),CancellationToken.None);
        Assert.Null(remove.Failure);Assert.NotNull(remove.Value);
        Assert.True(Assert.IsType<ExceptionSnapshot>(remove.Value.Target).IsRemoved);
        var history=await service.ExceptionHistoryAsync(new(Date:new DateOnly(2031,6,20),LineId:Guid.Parse(lineId)),CancellationToken.None);
        Assert.Null(history.Failure);Assert.NotNull(history.Value);Assert.Equal(2,history.Value.TotalCount);
    }

    [Fact]
    public async Task BusyHttpRequiresConfirmedRollbackAndDoesNotLeakLocalSettingsToPool()
    {
        using var client=await fixture.CreateClientAsAsync("Operator");
        var before=await fixture.ScalarAsOwnerAsync<long>("SELECT revision FROM plant_calendar_state");
        await using var owner=new NpgsqlConnection(fixture.OwnerConnectionString);await owner.OpenAsync();
        await using var held=await owner.BeginTransactionAsync();
        await using(var lockCommand=new NpgsqlCommand("SELECT id FROM plant_calendar_state WHERE id=1 FOR UPDATE",owner,held))await lockCommand.ExecuteScalarAsync();
        var response=await client.PutAsJsonAsync("/api/plant-calendar/exceptions",new{version=before.ToString(System.Globalization.CultureInfo.InvariantCulture),targetRevisionId=(string?)null,lineId=(string?)null,date="2031-06-21",isWorking=false,workingHours=(string?)null,reason=(string?)null});
        Assert.Equal(HttpStatusCode.ServiceUnavailable,response.StatusCode);
        var problem=await response.Body();Assert.Equal("CALENDAR_BUSY",problem["code"]!.GetValue<string>());Assert.Equal("NotApplied",problem["writeOutcome"]!.GetValue<string>());
        Assert.Equal(TimeSpan.FromSeconds(1),response.Headers.RetryAfter?.Delta);
        await held.RollbackAsync();
        Assert.Equal(0L,await fixture.ScalarAsOwnerAsync<long>("SELECT count(*) FROM plant_calendar_exception_revisions WHERE calendar_date='2031-06-21'"));
        await using var pooled=new NpgsqlConnection(fixture.AppConnectionString);await pooled.OpenAsync();
        foreach(var (setting,expected) in new[]{("lock_timeout","0"),("statement_timeout","0"),("transaction_read_only","off")})
        {
            await using var command=new NpgsqlCommand("SHOW "+setting,pooled);Assert.Equal(expected,await command.ExecuteScalarAsync());
        }
        Assert.Equal(HttpStatusCode.OK,(await client.GetAsync("/api/plant-calendar/month?month=2031-06")).StatusCode);
    }

    [Fact]
    public async Task UnrelatedConcurrentWritesHaveOneWinnerAndOneGlobalConflict()
    {
        var before=await fixture.ScalarAsOwnerAsync<long>("SELECT revision FROM plant_calendar_state");
        async Task<CalendarResult<MutationResult>> Save(int day)
        {
            using var scope=fixture.Services.CreateScope();
            return await scope.ServiceProvider.GetRequiredService<PlantCalendarService>().SaveExceptionAsync(new("SaveException",new DateOnly(2031,6,day),before,null,IsWorking:false),CancellationToken.None);
        }
        var results=await Task.WhenAll(Save(15),Save(16));
        Assert.Single(results,x=>x.Value is not null);
        var rejected=Assert.Single(results,x=>x.Failure is not null);
        Assert.Equal("CALENDAR_STALE",rejected.Failure?.Code);Assert.Equal("NotApplied",rejected.Failure?.WriteOutcome);
        Assert.Equal(before+1,await fixture.ScalarAsOwnerAsync<long>("SELECT revision FROM plant_calendar_state"));
        Assert.Equal(1L,await fixture.ScalarAsOwnerAsync<long>("SELECT count(*) FROM plant_calendar_exception_revisions WHERE calendar_date IN ('2031-06-15','2031-06-16')"));
    }
}
