using Microsoft.Extensions.Options;
using ProductionManagementAI.Application.ProductionOrders;

namespace ProductionManagementAI.Infrastructure.ProductionOrders;

public sealed class PlantOptions
{
    public const string SectionName = "Plant";

    /// <summary>IANA timezone ID that defines "today" and the order-number year (DEC-011, DEC-017).</summary>
    public string TimeZone { get; set; } = "Asia/Tokyo";

    public static bool IsValidTimeZone(string? id) =>
        !string.IsNullOrWhiteSpace(id) && TimeZoneInfo.TryFindSystemTimeZoneById(id, out _);
}

public sealed class PlantClock(TimeProvider timeProvider, IOptions<PlantOptions> options) : IPlantClock
{
    // Resolved once; an unknown ID is rejected at startup by options validation, not per request.
    private readonly TimeZoneInfo _zone = TimeZoneInfo.FindSystemTimeZoneById(options.Value.TimeZone);

    public DateOnly Today => DateOf(timeProvider.GetUtcNow());

    public short CurrentYear => (short)Today.Year;

    public string TimeZoneId { get; } = options.Value.TimeZone;

    public DateOnly DateOf(DateTimeOffset utc) => DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(utc, _zone).DateTime);

    public DateTimeOffset StartOfDayUtc(DateOnly date)
    {
        // Local midnight; if a DST gap skips it, the first valid local minute of that day (003_DD-FN §5).
        var local = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
        while (_zone.IsInvalidTime(local))
        {
            local = local.AddMinutes(1);
        }

        return new DateTimeOffset(local, _zone.GetUtcOffset(local)).ToUniversalTime();
    }
}
