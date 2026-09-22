using ProductionManagementAI.Domain.ProductionOrders;

namespace ProductionManagementAI.Application.ProductionOrders;

/// <summary>Plant-local date and year in the configured timezone (DEC-011, DEC-017).</summary>
public interface IPlantClock
{
    DateOnly Today { get; }

    short CurrentYear { get; }

    /// <summary>The configured IANA timezone ID; passed to PostgreSQL where SQL converts to plant dates (DD-003-FN §5).</summary>
    string TimeZoneId { get; }

    /// <summary>The plant-local date of an instant.</summary>
    DateOnly DateOf(DateTimeOffset utc);

    /// <summary>The UTC instant at which a plant-local date begins.</summary>
    DateTimeOffset StartOfDayUtc(DateOnly date);
}

/// <summary>Issues the next per-year order sequence inside the caller's transaction (DB-002, DEC-013).</summary>
public interface IOrderNumberIssuer
{
    Task<int> NextAsync(short year, CancellationToken cancellationToken);
}

public interface IProductionOrderTransaction : IAsyncDisposable
{
    Task CommitAsync(CancellationToken cancellationToken);
}

public interface IProductionOrderRepository
{
    Task<IReadOnlyList<ProductResponse>> ListProductsAsync(CancellationToken cancellationToken);

    Task<bool> ProductExistsAsync(Guid productId, CancellationToken cancellationToken);

    Task<ProductionOrder?> FindAsync(Guid id, bool tracked, CancellationToken cancellationToken);

    /// <summary>Exact number of orders matching the query's filters, ignoring sort and paging (DD-002-FN §2).</summary>
    Task<int> CountOrdersAsync(ProductionOrderListQuery query, CancellationToken cancellationToken);

    /// <summary>One ordered, projected page of orders (DD-002-FN §3).</summary>
    Task<IReadOnlyList<ProductionOrderListRow>> ListOrdersAsync(
        ProductionOrderListQuery query, CancellationToken cancellationToken);

    void Add(ProductionOrder order);

    Task<IProductionOrderTransaction> BeginTransactionAsync(CancellationToken cancellationToken);

    /// <exception cref="ConcurrencyConflictException">The row changed since it was loaded (DEC-010, DEC-014).</exception>
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

/// <summary>Raised by the repository when an optimistic-concurrency check fails.</summary>
public sealed class ConcurrencyConflictException(Exception? inner = null)
    : Exception("The production order was changed by someone else.", inner);
