using ProductionManagementAI.Application.Dashboard;

namespace ProductionManagementAI.Application.Tests.Dashboard;

/// <summary>BD-003 D-01–D-09 calendar rules (TC-205, TC-209, unit), for every weekday and at a month boundary.</summary>
public class DashboardWindowTests
{
    private static readonly TokyoClock Clock = new(TimeProvider.System);

    [Theory]
    [InlineData("2026-09-21")] // Monday
    [InlineData("2026-09-22")]
    [InlineData("2026-09-23")]
    [InlineData("2026-09-24")]
    [InlineData("2026-09-25")]
    [InlineData("2026-09-26")]
    [InlineData("2026-09-27")] // Sunday
    public void Week0_is_the_Monday_on_or_before_today_for_every_weekday(string today)
    {
        var window = DashboardWindow.For(DateOnly.Parse(today), Clock);

        Assert.Equal(new DateOnly(2026, 9, 21), window.Week0);
        Assert.Equal(DayOfWeek.Monday, window.Week0.DayOfWeek);
    }

    [Fact]
    public void Windows_follow_DEC_007_and_DEC_008()
    {
        var today = new DateOnly(2026, 9, 23);

        var window = DashboardWindow.For(today, Clock);

        Assert.Equal(new DateOnly(2026, 9, 30), window.SoonEnd); // today + 7
        Assert.Equal(new DateOnly(2026, 8, 25), window.WindowStart); // today − 29: 30 days, today included
        Assert.Equal(new DateOnly(2026, 9, 1), window.MonthStart);
        Assert.Equal(12, window.TrendWeeks.Count);
        Assert.Equal(window.Week0, window.TrendWeeks[^1]);
        Assert.Equal(window.Week0.AddDays(-77), window.TrendWeeks[0]);
        Assert.All(window.TrendWeeks, monday => Assert.Equal(DayOfWeek.Monday, monday.DayOfWeek));
        Assert.Equal("Asia/Tokyo", window.TimeZoneId);
    }

    [Fact]
    public void Utc_bounds_are_plant_midnights_not_utc_midnights()
    {
        var window = DashboardWindow.For(new DateOnly(2026, 9, 23), Clock);

        // 2026-09-21 00:00 JST is 2026-09-20 15:00 UTC.
        Assert.Equal(new DateTimeOffset(2026, 9, 20, 15, 0, 0, TimeSpan.Zero), window.WeekStartUtc);
        Assert.Equal(new DateTimeOffset(2026, 8, 31, 15, 0, 0, TimeSpan.Zero), window.MonthStartUtc);
        Assert.Equal(new DateTimeOffset(2026, 8, 24, 15, 0, 0, TimeSpan.Zero), window.WindowStartUtc);
        Assert.Equal(new DateTimeOffset(2026, 9, 23, 15, 0, 0, TimeSpan.Zero), window.EndUtc);
        Assert.Equal(new DateTimeOffset(2026, 7, 5, 15, 0, 0, TimeSpan.Zero), window.TrendStartUtc);
        Assert.Equal(TimeSpan.Zero, window.EndUtc.Offset);
    }

    [Fact]
    public void Month_start_on_the_first_of_the_month_is_today()
    {
        var window = DashboardWindow.For(new DateOnly(2026, 10, 1), Clock);

        Assert.Equal(new DateOnly(2026, 10, 1), window.MonthStart);
        Assert.Equal(new DateOnly(2026, 9, 28), window.Week0); // a Thursday; the week began in September
    }
}
