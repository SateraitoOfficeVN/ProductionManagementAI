using ProductionManagementAI.Domain.ProductionLines;
using ProductionManagementAI.Domain.ProductionOrders;

namespace ProductionManagementAI.Application.ProductionLines;

/// <summary>Returns a fixed-size snapshot page without sorting metadata.</summary>
public sealed record LinePage<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize = 50);
/// <summary>Returns current production-line identity and an opaque concurrency token.</summary>
public record LineSummary(Guid Id, string Code, string Name, string WorkingHoursPerDay,
    bool IsActive, string UpdatedAt, string Version);
/// <summary>Returns current line identity and a snapshot page of all associated products.</summary>
public sealed record LineDetail(Guid Id, string Code, string Name, string WorkingHoursPerDay,
    bool IsActive, string UpdatedAt, string Version, LinePage<LinePairDetail> Pairs)
    : LineSummary(Id, Code, Name, WorkingHoursPerDay, IsActive, UpdatedAt, Version);
/// <summary>Returns a product observation with its exact unit generation.</summary>
public sealed record LineProductChoice(Guid Id, string Sku, string Name, string Unit,
    string UnitRevision, bool IsActive);
/// <summary>Returns current timing, preserved confirmation and retirement state.</summary>
public sealed record LinePairDetail(LineProductChoice Product, string MinutesPerUnit,
    string ConfirmedUnit, string ConfirmedUnitRevision, bool IsActive,
    bool RequiresUnitConfirmation, string UpdatedAt);
/// <summary>Returns an eligible line with its coefficient for the observed product unit.</summary>
public sealed record EligibleLine(Guid Id, string Code, string Name, string WorkingHoursPerDay,
    bool IsActive, string UpdatedAt, string Version, string MinutesPerUnit, string Unit)
    : LineSummary(Id, Code, Name, WorkingHoursPerDay, IsActive, UpdatedAt, Version);
/// <summary>Returns eligible rows and their product observation from one snapshot.</summary>
public sealed record EligibleLinePage(LineProductChoice Product, IReadOnlyList<EligibleLine> Items,
    int Total, int Page, int PageSize = 50);
/// <summary>Defines untrusted line list filters.</summary>
public sealed record LineListRequest(string? Q, string? State, string? Page);
/// <summary>Defines one untrusted association action; retirement has no timing fields.</summary>
public sealed record LineProductInput(string Action, Guid ProductId, string? MinutesPerUnit = null,
    string? ExpectedUnit = null, string? ExpectedUnitRevision = null, bool? ConfirmUnit = null);
/// <summary>Defines a submitted new line and required product action array.</summary>
public sealed record CreateLineRequest(string? Code, string? Name, string? WorkingHoursPerDay,
    IReadOnlyList<LineProductInput>? Products);
/// <summary>Defines a submitted aggregate edit and opaque expected version.</summary>
public sealed record UpdateLineRequest(string? Name, string? WorkingHoursPerDay, string? Version,
    IReadOnlyList<LineProductInput>? ProductChanges);
/// <summary>Defines a validated paging and literal-search query.</summary>
public sealed record LineQuery(string? Search, int Page, string State = "active", Guid? TargetId = null);
/// <summary>Defines a validated association intent, retaining its submitted field prefix.</summary>
public sealed record LineProductChange(string Action, Guid ProductId, decimal MinutesPerUnit,
    string ExpectedUnit, long ExpectedUnitRevision, bool ConfirmUnit, string Field);
/// <summary>Defines a normalized, validated atomic master command.</summary>
public sealed record LineCommand(string Code, string Name, decimal WorkingHoursPerDay,
    uint Version, IReadOnlyList<LineProductChange> Changes);
/// <summary>Returns a bounded known application failure without submitted values.</summary>
public sealed record LineProblem(int Status, string Code, IReadOnlyDictionary<string, string[]>? Errors = null);
/// <summary>Returns either a successful projection or a classified feature failure.</summary>
public sealed record LineResult<T>(T? Value, LineProblem? Problem)
{
    /// <summary>Creates a successful result.</summary>
    public static LineResult<T> Success(T value) => new(value, null);
    /// <summary>Creates a classified failure with an optional stable field path.</summary>
    public static LineResult<T> Failure(int status, string code, string? field = null) =>
        new(default, new(status, code, field is null ? null : new Dictionary<string, string[]> { [field] = [code] }));
}
/// <summary>Supplies snapshot reads and bounded atomic write sessions.</summary>
public interface IProductionLineRepository
{
    /// <summary>Reads line count and rows from one read-only snapshot.</summary>
    Task<LinePage<LineSummary>> ListAsync(LineQuery query, CancellationToken cancellationToken);
    /// <summary>Reads line identity and a pair page from one read-only snapshot.</summary>
    Task<LineDetail?> GetAsync(Guid id, int pairsPage, CancellationToken cancellationToken);
    /// <summary>Reads active product choices excluding all persisted associations.</summary>
    Task<LinePage<LineProductChoice>?> ProductChoicesAsync(LineQuery query, CancellationToken cancellationToken);
    /// <summary>Reads currently eligible lines and the product from one snapshot.</summary>
    Task<EligibleLinePage?> EligibleAsync(LineQuery query, CancellationToken cancellationToken);
    /// <summary>Begins a write session with transaction-local bounded waits.</summary>
    Task<ILineWriteSession> BeginWriteAsync(CancellationToken cancellationToken);
}
/// <summary>Holds an atomic aggregate transaction and canonical lock ordering.</summary>
public interface ILineWriteSession : IAsyncDisposable
{
    /// <summary>Locks products for shared access in canonical UUID order and reads fresh values.</summary>
    Task<IReadOnlyDictionary<Guid, Product>> LockProductsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken);
    /// <summary>Locks the parent before its touched pair rows for exclusive aggregate mutation.</summary>
    Task<ProductionLine?> LockLineAsync(Guid id, CancellationToken cancellationToken);
    /// <summary>Locks touched pairs after their parent, retaining retired rows.</summary>
    Task<IReadOnlyDictionary<Guid, ProductionLineProduct>> LockPairsAsync(Guid lineId, IEnumerable<Guid> productIds, CancellationToken cancellationToken);
    /// <summary>Tracks a newly validated line.</summary>
    void Add(ProductionLine line);
    /// <summary>Tracks a newly validated association.</summary>
    void Add(ProductionLineProduct pair);
    /// <summary>Forces a parent update even when the supplied audit instant is unchanged.</summary>
    void Touch(ProductionLine line, DateTimeOffset now);
    /// <summary>Persists all validated changes once.</summary>
    Task SaveAsync(CancellationToken cancellationToken);
    /// <summary>Commits once without automatic retry.</summary>
    Task CommitAsync(CancellationToken cancellationToken);
    /// <summary>Confirms rollback with a fresh bounded cleanup token.</summary>
    Task<bool> RollbackAsync(CancellationToken cancellationToken);
}
/// <summary>Reports a named immutable line-code uniqueness conflict.</summary>
public sealed class LineCodeConflictException(Exception inner) : Exception("Line code conflict.", inner);
/// <summary>Reports an aggregate optimistic concurrency conflict.</summary>
public sealed class LineVersionConflictException(Exception inner) : Exception("Line version conflict.", inner);
/// <summary>Reports a provider-proven lock, statement timeout or deadlock before commit.</summary>
public sealed class LineTransientException(Exception inner) : Exception("Line operation is busy.", inner);
