namespace ProductionManagementAI.Domain.PlantCalendar;

/// <summary>Stores one retained weekly payload or withdrawal marker.</summary>
public sealed class WeeklyRevision
{
    /// <summary>Gets the immutable snapshot identity.</summary>
    public Guid Id { get; init; }
    /// <summary>Gets the parent calendar identity.</summary>
    public short CalendarId { get; init; } = 1;
    /// <summary>Gets the immutable logical start date.</summary>
    public DateOnly EffectiveFrom { get; init; }
    /// <summary>Gets the weekday mask, or null for withdrawal.</summary>
    public short? WorkingWeekdays { get; init; }
    /// <summary>Gets whether this payload is a withdrawal marker.</summary>
    public bool IsWithdrawn { get; init; }
    /// <summary>Gets or sets whether this is the latest target snapshot.</summary>
    public bool IsCurrent { get; set; } = true;
    /// <summary>Gets the recording aggregate version.</summary>
    public long CommitRevision { get; init; }
    /// <summary>Gets the immutable recording instant.</summary>
    public DateTimeOffset CreatedAtUtc { get; init; }
}
