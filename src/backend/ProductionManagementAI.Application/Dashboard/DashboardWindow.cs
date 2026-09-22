using ProductionManagementAI.Application.ProductionOrders;

namespace ProductionManagementAI.Application.Dashboard;

/// <summary>
/// Every date and UTC instant the dashboard's figures depend on, derived from the plant-local date T
/// (DD-003-FN §2, BD-003 D-01–D-09, DB-004's parameter table). A pure value, so every calendar rule is unit-tested
/// for each weekday and month boundary without a database.
/// </summary>
public sealed record DashboardWindow(
    DateOnly Today,
    DateOnly SoonEnd,
    DateOnly Week0,
    DateOnly MonthStart,
    DateOnly WindowStart,
    IReadOnlyList<DateOnly> TrendWeeks,
    DateTimeOffset TrendStartUtc,
    DateTimeOffset WeekStartUtc,
    DateTimeOffset MonthStartUtc,
    DateTimeOffset WindowStartUtc,
    DateTimeOffset EndUtc,
    string TimeZoneId)
{
    public const int DueSoonDays = 7; // DEC-007
    public const int PerformanceWindowDays = 30; // DEC-008: today and the 29 days before it
    public const int TrendWeekCount = 12; // DEC-008
    public const int LookAheadWeeks = 8; // DEC-008: the current week and the next 7
    public const int GroupRowLimit = 10; // DEC-008
    public const int TopProductLimit = 10; // DEC-008

    public static DashboardWindow For(DateOnly today, IPlantClock clock)
    {
        // Monday on or before today: DayOfWeek counts Sunday as 0, so Sunday maps to 6 days back (DEC-008).
        var week0 = today.AddDays(-(((int)today.DayOfWeek + 6) % 7));
        var monthStart = new DateOnly(today.Year, today.Month, 1);
        var windowStart = today.AddDays(-(PerformanceWindowDays - 1));
        var trendWeeks = Enumerable.Range(0, TrendWeekCount)
            .Select(k => week0.AddDays(-7 * (TrendWeekCount - 1 - k)))
            .ToArray();

        return new DashboardWindow(
            today,
            today.AddDays(DueSoonDays),
            week0,
            monthStart,
            windowStart,
            trendWeeks,
            clock.StartOfDayUtc(trendWeeks[0]),
            clock.StartOfDayUtc(week0),
            clock.StartOfDayUtc(monthStart),
            clock.StartOfDayUtc(windowStart),
            clock.StartOfDayUtc(today.AddDays(1)),
            clock.TimeZoneId);
    }
}
