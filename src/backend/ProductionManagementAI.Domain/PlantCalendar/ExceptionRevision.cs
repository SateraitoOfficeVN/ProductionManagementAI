namespace ProductionManagementAI.Domain.PlantCalendar;

/// <summary>Stores one retained date exception or removal marker.</summary>
public sealed class ExceptionRevision
{
    /// <summary>Gets the immutable snapshot identity.</summary>
    public Guid Id { get; init; }
    /// <summary>Gets the parent calendar identity.</summary>
    public short CalendarId { get; init; } = 1;
    /// <summary>Gets the immutable optional line scope.</summary>
    public Guid? LineId { get; init; }
    /// <summary>Gets the immutable exception date.</summary>
    public DateOnly CalendarDate { get; init; }
    /// <summary>Gets the working state, or null for removal.</summary>
    public bool? IsWorking { get; init; }
    /// <summary>Gets explicit hours, or null for inherited or closed state.</summary>
    public decimal? WorkingHours { get; init; }
    /// <summary>Gets the normalized optional plain-text reason.</summary>
    public string? Reason { get; init; }
    /// <summary>Gets whether this is a removal marker.</summary>
    public bool IsRemoved { get; init; }
    /// <summary>Gets or sets whether this is the latest target snapshot.</summary>
    public bool IsCurrent { get; set; } = true;
    /// <summary>Gets the recording aggregate version.</summary>
    public long CommitRevision { get; init; }
    /// <summary>Gets the immutable recording instant.</summary>
    public DateTimeOffset CreatedAtUtc { get; init; }
}
