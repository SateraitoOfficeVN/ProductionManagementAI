namespace ProductionManagementAI.Domain.PlantCalendar;

/// <summary>Stores the immutable activation basis and mutable aggregate revision.</summary>
public sealed class CalendarState
{
    /// <summary>Gets the singleton identity.</summary>
    public short Id { get; init; } = 1;
    /// <summary>Gets the immutable first covered date.</summary>
    public DateOnly ActivatedOn { get; init; }
    /// <summary>Gets the immutable configured date basis.</summary>
    public string TimeZoneId { get; init; } = string.Empty;
    /// <summary>Gets or sets the durable aggregate version.</summary>
    public long Revision { get; set; } = 1;
    /// <summary>Gets the immutable recording instant.</summary>
    public DateTimeOffset CreatedAtUtc { get; init; }
    /// <summary>Gets or sets the last committed change instant.</summary>
    public DateTimeOffset UpdatedAtUtc { get; set; }
}
