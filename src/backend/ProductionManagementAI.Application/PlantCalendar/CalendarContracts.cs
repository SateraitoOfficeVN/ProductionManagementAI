namespace ProductionManagementAI.Application.PlantCalendar;

/// <summary>Returns one captured plant date and aggregate observation.</summary>
public sealed record CalendarContext(DateOnly PlantToday, string TimeZone, DateOnly? ActivatedOn, string? Version);

/// <summary>Returns current or retained line identity without an edit token.</summary>
public sealed record CalendarLine(Guid Id, string Code, string Name, bool IsActive, string WorkingHoursPerDay);

/// <summary>Returns current product identity and exact unit generation.</summary>
public sealed record CalendarProduct(Guid Id, string Sku, string Name, string Unit, string UnitRevision);

/// <summary>Returns the requested plant or retained line scope.</summary>
public sealed record CalendarScope(Guid? LineId, CalendarLine? Line);

/// <summary>Returns one winning or fallback immutable rule reference.</summary>
public sealed record CalendarSource(string Kind, Guid? RevisionId, DateOnly? EffectiveFrom, string? Reason);

/// <summary>Returns one resolved date without claiming historical capacity.</summary>
public sealed record DaySummary(DateOnly Date, string State, string? Hours, string HoursBasis, CalendarSource Source, IReadOnlyList<CalendarSource> Fallback, bool Editable);

/// <summary>Returns an immutable weekly payload with current-head observation.</summary>
public sealed record WeeklySnapshot(Guid Id, DateOnly EffectiveFrom, IReadOnlyList<string>? WorkingDays, bool IsWithdrawn, bool IsCurrent, string CommitRevision, DateTimeOffset CreatedAtUtc);

/// <summary>Returns an immutable date payload or retained removal marker.</summary>
public sealed record ExceptionSnapshot(Guid Id, Guid? LineId, DateOnly Date, bool? IsWorking, string? WorkingHours, string? Reason, bool IsRemoved, bool IsCurrent, string CommitRevision, DateTimeOffset CreatedAtUtc);

/// <summary>Returns one coherent calendar month.</summary>
public sealed record MonthResult(CalendarContext Context, CalendarScope Scope, string Month, IReadOnlyList<DaySummary> Days);

/// <summary>Returns a resolved day and its exact editable scope target.</summary>
public sealed record DayResult(CalendarContext Context, CalendarScope Scope, DaySummary Day, ExceptionSnapshot? Exception);

/// <summary>Returns a version-bound weekly range and predecessor.</summary>
public sealed record WeeklyPage(CalendarContext Context, string? SnapshotVersion, int Page, int PageSize, int TotalCount, int TotalPages, IReadOnlyList<WeeklySnapshot> Items, WeeklySnapshot? ApplicableBeforeFrom);

/// <summary>Returns version-bound retained history and the current target.</summary>
public sealed record ExceptionPage(CalendarContext Context, CalendarScope Scope, DateOnly Date, string? SnapshotVersion, int Page, int PageSize, int TotalCount, int TotalPages, IReadOnlyList<ExceptionSnapshot> Items, ExceptionSnapshot? Current);

/// <summary>Returns a page of active line choices.</summary>
public sealed record LineChoicePage(CalendarContext Context, int Page, int PageSize, int TotalCount, int TotalPages, IReadOnlyList<CalendarLine> Items);

/// <summary>Returns a page of current eligible product choices.</summary>
public sealed record ProductChoicePage(CalendarContext Context, int Page, int PageSize, int TotalCount, int TotalPages, IReadOnlyList<CalendarProduct> Items);

/// <summary>Returns current unit-bearing capacity or explicit unavailability.</summary>
public sealed record CapacityResult(CalendarContext Context, CalendarLine Line, CalendarProduct Product, DateOnly Date, string Availability, string UnavailableReason, DaySummary Day, string? MinutesPerUnit, string? Quantity);

/// <summary>Returns an acknowledged retained transition or authoritative no-op.</summary>
public sealed record MutationResult(CalendarContext Context, bool Changed, object Target);

/// <summary>Carries strictly parsed query parameters without arbitrary SQL.</summary>
public sealed record CalendarReadQuery(DateOnly Date = default, DateOnly From = default, DateOnly To = default, Guid? LineId = null, Guid? ProductId = null, string? Search = null, int Page = 1, string View = "Current", string? SnapshotVersion = null);

/// <summary>Carries one captured save, removal or withdrawal intent.</summary>
public sealed record CalendarCommand(string Kind, DateOnly Date, long Version, Guid? TargetRevisionId, Guid? LineId = null, short? Mask = null, bool? IsWorking = null, decimal? Hours = null, string? Reason = null);

/// <summary>Returns a bounded raw projection page.</summary>
public sealed record RecordPage<T>(IReadOnlyList<T> Items, int Total);

/// <summary>Returns either a typed value or a safe feature failure.</summary>
public sealed record CalendarResult<T>(T? Value, CalendarFailure? Failure) where T : class;

/// <summary>Describes a safe typed HTTP failure and write certainty.</summary>
public sealed record CalendarFailure(string Code, int Status, string? WriteOutcome = null, IReadOnlyDictionary<string, string[]>? Errors = null);

/// <summary>Propagates a typed failure within the transaction boundary without exposing internals.</summary>
public sealed class CalendarRuleException(CalendarFailure failure) : Exception(failure.Code)
{
    /// <summary>Gets the safe transport failure.</summary>
    public CalendarFailure Failure { get; } = failure;
}
