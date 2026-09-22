using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ProductionManagementAI.Application.ProductionOrders;
using ProductionManagementAI.Domain.ProductionOrders;

namespace ProductionManagementAI.Infrastructure.ProductionOrders;

internal sealed class ProductionOrderRepository(AppDbContext db) : IProductionOrderRepository
{
    public async Task<IReadOnlyList<ProductResponse>> ListProductsAsync(CancellationToken cancellationToken) =>
        await db.Products
            .AsNoTracking()
            .OrderBy(p => p.Sku)
            .Select(p => new ProductResponse(p.Id, p.Sku, p.Name))
            .ToListAsync(cancellationToken);

    public Task<bool> ProductExistsAsync(Guid productId, CancellationToken cancellationToken) =>
        db.Products.AnyAsync(p => p.Id == productId, cancellationToken);

    public Task<ProductionOrder?> FindAsync(Guid id, bool tracked, CancellationToken cancellationToken)
    {
        var query = tracked ? db.ProductionOrders : db.ProductionOrders.AsNoTracking();
        return query.FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    // DD-002-FN §2: filters only, and no join — every list filter is a predicate on production_orders (DB-003).
    public Task<int> CountOrdersAsync(ProductionOrderListQuery query, CancellationToken cancellationToken) =>
        db.ProductionOrders.AsNoTracking().ApplyFilters(query).CountAsync(cancellationToken);

    // DD-002-FN §3: filter, join for the displayed product, project, order, then page. Notes, xmin, order_year,
    // order_seq and created_at_utc are never selected (DB-003 read projection).
    public async Task<IReadOnlyList<ProductionOrderListRow>> ListOrdersAsync(
        ProductionOrderListQuery query, CancellationToken cancellationToken) =>
        await db.ProductionOrders
            .AsNoTracking()
            .ApplyFilters(query)
            .Join(
                db.Products.AsNoTracking(),
                order => order.ProductId,
                product => product.Id,
                (order, product) => new ProductionOrderListRow(
                    order.Id,
                    order.OrderNumber,
                    product.Id,
                    product.Sku,
                    product.Name,
                    order.Quantity,
                    order.DueDate,
                    order.Status,
                    order.UpdatedAtUtc))
            .ApplySort(query.Sort, query.Direction)
            .Skip(query.Skip)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

    public void Add(ProductionOrder order) => db.ProductionOrders.Add(order);

    public async Task<IProductionOrderTransaction> BeginTransactionAsync(CancellationToken cancellationToken) =>
        new Transaction(await db.Database.BeginTransactionAsync(cancellationToken));

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            // The generated order_number and the new xmin come back via RETURNING, so the entity is current afterwards.
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConcurrencyConflictException(ex);
        }
    }

    private sealed class Transaction(IDbContextTransaction inner) : IProductionOrderTransaction
    {
        public Task CommitAsync(CancellationToken cancellationToken) => inner.CommitAsync(cancellationToken);

        public ValueTask DisposeAsync() => inner.DisposeAsync();
    }
}

/// <summary>DB-002 counter upsert, run inside the caller's transaction (DEC-013).</summary>
internal sealed class OrderNumberIssuer(AppDbContext db) : IOrderNumberIssuer
{
    public async Task<int> NextAsync(short year, CancellationToken cancellationToken)
    {
        if (db.Database.CurrentTransaction is null)
        {
            // Issuing outside the insert's transaction could consume a number that is never used.
            throw new InvalidOperationException("Order numbers must be issued inside the create transaction.");
        }

        // Interpolated SQL is sent as parameters by EF Core. ToListAsync (not SingleAsync) because EF must not
        // wrap a non-SELECT statement in a subquery.
        var values = await db.Database.SqlQuery<int>($"""
            INSERT INTO production_order_number_counters (order_year, last_seq)
            VALUES ({year}, 1)
            ON CONFLICT (order_year) DO UPDATE SET last_seq = production_order_number_counters.last_seq + 1
            RETURNING last_seq AS "Value"
            """).ToListAsync(cancellationToken);

        return values.Single();
    }
}
