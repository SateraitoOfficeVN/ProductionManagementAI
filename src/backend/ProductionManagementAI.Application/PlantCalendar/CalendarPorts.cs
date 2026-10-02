using ProductionManagementAI.Domain.PlantCalendar;
using ProductionManagementAI.Domain.ProductionLines;
using ProductionManagementAI.Domain.ProductionOrders;

namespace ProductionManagementAI.Application.PlantCalendar;

/// <summary>Provides projected reads within one caller-owned coherent snapshot.</summary>
public interface ICalendarReader
{
    /// <summary>Reads a retained line identity.</summary>
    Task<ProductionLine?> LineAsync(Guid id, CancellationToken ct);
    /// <summary>Reads a retained product identity.</summary>
    Task<Product?> ProductAsync(Guid id, CancellationToken ct);
    /// <summary>Reads an optional current timing.</summary>
    Task<ProductionLineProduct?> PairAsync(Guid lineId, Guid productId, CancellationToken ct);
    /// <summary>Reads the latest surviving weekly start on or before a date.</summary>
    Task<WeeklyRevision?> ApplicableWeeklyAsync(DateOnly date, CancellationToken ct);
    /// <summary>Reads batched live starts inside a month.</summary>
    Task<IReadOnlyList<WeeklyRevision>> MonthWeeklyAsync(DateOnly from, DateOnly to, CancellationToken ct);
    /// <summary>Reads batched current plant and optional line exception heads.</summary>
    Task<IReadOnlyList<ExceptionRevision>> MonthExceptionsAsync(DateOnly from, DateOnly to, Guid? lineId, CancellationToken ct);
    /// <summary>Reads an exact target including its latest marker.</summary>
    Task<ExceptionRevision?> ExceptionAsync(DateOnly date, Guid? lineId, CancellationToken ct);
    /// <summary>Reads bounded current or historical weekly rows.</summary>
    Task<RecordPage<WeeklyRevision>> WeeklyPageAsync(CalendarReadQuery query, CancellationToken ct);
    /// <summary>Reads bounded history for an exact nullable scope and date.</summary>
    Task<RecordPage<ExceptionRevision>> ExceptionPageAsync(CalendarReadQuery query, CancellationToken ct);
    /// <summary>Reads a bounded page of active lines.</summary>
    Task<RecordPage<ProductionLine>> LineChoicesAsync(CalendarReadQuery query, CancellationToken ct);
    /// <summary>Reads a bounded page of current eligible products.</summary>
    Task<RecordPage<Product>> ProductChoicesAsync(CalendarReadQuery query, CancellationToken ct);
}

/// <summary>Owns coherent reads and acknowledged retained write transitions.</summary>
public interface ICalendarRepository
{
    /// <summary>Runs a projection inside one read-only repeatable snapshot.</summary>
    Task<T> ReadSnapshotAsync<T>(Func<ICalendarReader, CalendarContext, CancellationToken, Task<T>> read, CancellationToken ct);
    /// <summary>Applies one captured command with the approved locks and commit certainty.</summary>
    Task<MutationResult> WriteTransitionAsync(CalendarCommand command, CancellationToken ct);
}
