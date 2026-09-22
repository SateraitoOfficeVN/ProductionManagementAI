using ProductionManagementAI.Application.ProductionOrders;

namespace ProductionManagementAI.Application.Tests.Dashboard;

/// <summary>An IPlantClock on the real Asia/Tokyo zone, for calendar tests. Mirrors PlantClock's conversions.</summary>
internal sealed class TokyoClock(TimeProvider time) : IPlantClock
{
    private static readonly TimeZoneInfo Zone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Tokyo");

    public DateOnly Today => DateOf(time.GetUtcNow());

    public short CurrentYear => (short)Today.Year;

    public string TimeZoneId => "Asia/Tokyo";

    public DateOnly DateOf(DateTimeOffset utc) => DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(utc, Zone).DateTime);

    public DateTimeOffset StartOfDayUtc(DateOnly date)
    {
        var local = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
        return new DateTimeOffset(local, Zone.GetUtcOffset(local)).ToUniversalTime();
    }
}
