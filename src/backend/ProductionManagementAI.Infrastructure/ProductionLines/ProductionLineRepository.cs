using System.Data;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using ProductionManagementAI.Application.ProductionLines;
using ProductionManagementAI.Domain.ProductionLines;
using ProductionManagementAI.Domain.ProductionOrders;

namespace ProductionManagementAI.Infrastructure.ProductionLines;

internal sealed class ProductionLineRepository(AppDbContext db) : IProductionLineRepository
{
    public Task<LinePage<LineSummary>> ListAsync(LineQuery query, CancellationToken ct) => Snapshot(async token =>
    {
        var rows = FilterLines(query);
        var total = await rows.CountAsync(token);
        var lines = await rows.OrderBy(l => l.Code).ThenBy(l => l.Id).Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(token);
        return new LinePage<LineSummary>(lines.Select(Summary).ToList(), total, query.Page, query.PageSize);
    }, ct);

    public Task<LineDetail?> GetAsync(Guid id, int pairsPage, int pairsPageSize, CancellationToken ct) => Snapshot<LineDetail?>(async token =>
    {
        var line = await db.ProductionLines.AsNoTracking().SingleOrDefaultAsync(l => l.Id == id, token);
        if (line is null) return null;
        var query = from pair in db.ProductionLineProducts.AsNoTracking()
                    join product in db.Products.AsNoTracking() on pair.ProductId equals product.Id
                    where pair.LineId == id
                    select new { Pair = pair, Product = product };
        var total = await query.CountAsync(token);
        var rows = await query.OrderBy(r => r.Product.Sku).ThenBy(r => r.Product.Id)
            .Skip((pairsPage - 1) * pairsPageSize).Take(pairsPageSize).ToListAsync(token);
        var pairs = rows.Select(r => new LinePairDetail(ProductChoice(r.Product), LineValueRules.Format(r.Pair.MinutesPerUnit),
            r.Pair.ConfirmedUnit, Revision(r.Pair.ConfirmedUnitRevision), r.Pair.IsActive,
            r.Pair.ConfirmedUnit != r.Product.Unit || r.Pair.ConfirmedUnitRevision != r.Product.UnitRevision, Utc(r.Pair.UpdatedAtUtc))).ToList();
        var summary = Summary(line);
        return new LineDetail(summary.Id, summary.Code, summary.Name, summary.WorkingHoursPerDay,
            summary.IsActive, summary.UpdatedAt, summary.Version, new(pairs, total, pairsPage, pairsPageSize));
    }, ct);

    public Task<LinePage<LineProductChoice>?> ProductChoicesAsync(LineQuery query, CancellationToken ct) => Snapshot<LinePage<LineProductChoice>?>(async token =>
    {
        if (query.TargetId is { } lineId && !await db.ProductionLines.AnyAsync(l => l.Id == lineId, token)) return null;
        var products = db.Products.AsNoTracking().Where(p => p.IsActive);
        if (query.TargetId is { } id) products = products.Where(p => !db.ProductionLineProducts.Any(pair => pair.LineId == id && pair.ProductId == p.Id));
        if (query.Search is { } search)
        {
            var pattern = Pattern(search);
            products = products.Where(p => EF.Functions.ILike(p.Sku, pattern, "\\") || EF.Functions.ILike(p.Name, pattern, "\\"));
        }
        var total = await products.CountAsync(token);
        var rows = await products.OrderBy(p => p.Sku).ThenBy(p => p.Id).Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(token);
        return new LinePage<LineProductChoice>(rows.Select(ProductChoice).ToList(), total, query.Page, query.PageSize);
    }, ct);

    public Task<EligibleLinePage?> EligibleAsync(LineQuery query, CancellationToken ct) => Snapshot<EligibleLinePage?>(async token =>
    {
        var product = await db.Products.AsNoTracking().SingleOrDefaultAsync(p => p.Id == query.TargetId, token);
        if (product is null) return null;
        var lines = FilterLines(query with { State = "active" });
        var candidates = from pair in db.ProductionLineProducts.AsNoTracking()
                         join line in lines on pair.LineId equals line.Id
                         where product.IsActive && pair.ProductId == product.Id && pair.IsActive &&
                            pair.ConfirmedUnit == product.Unit && pair.ConfirmedUnitRevision == product.UnitRevision
                         select new { Line = line, Pair = pair };
        var total = await candidates.CountAsync(token);
        var rows = await candidates.OrderBy(r => r.Line.Code).ThenBy(r => r.Line.Id).Skip((query.Page - 1) * 50).Take(50).ToListAsync(token);
        return new EligibleLinePage(ProductChoice(product), rows.Select(r => {
            var line = Summary(r.Line);
            return new EligibleLine(line.Id, line.Code, line.Name, line.WorkingHoursPerDay, line.IsActive,
                line.UpdatedAt, line.Version, LineValueRules.Format(r.Pair.MinutesPerUnit), product.Unit);
        }).ToList(), total, query.Page);
    }, ct);

    public async Task<ILineWriteSession> BeginWriteAsync(CancellationToken ct)
    {
        var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);
        db.Database.AutoSavepointsEnabled = false;
        var session = new WriteSession(db, transaction);
        try { await Limits(false, ct); return session; }
        catch (Exception exception)
        {
            using var cleanup = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            var rolledBack = await session.RollbackAsync(cleanup.Token);
            await session.DisposeAsync();
            if (rolledBack && IsTransient(exception)) throw new LineTransientException(exception);
            throw;
        }
    }

    private IQueryable<ProductionLine> FilterLines(LineQuery query)
    {
        var lines = db.ProductionLines.AsNoTracking().AsQueryable();
        if (query.State == "active") lines = lines.Where(l => l.IsActive);
        if (query.State == "retired") lines = lines.Where(l => !l.IsActive);
        if (query.Search is { } search)
        {
            var pattern = Pattern(search);
            lines = lines.Where(l => EF.Functions.ILike(l.Code, pattern, "\\") || EF.Functions.ILike(l.Name, pattern, "\\"));
        }
        return lines;
    }

    private async Task<T> Snapshot<T>(Func<CancellationToken, Task<T>> read, CancellationToken ct)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead, ct);
        try
        {
            await Limits(true, ct);
            var value = await read(ct);
            await transaction.CommitAsync(ct);
            return value;
        }
        catch (Exception exception)
        {
            using var cleanup = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            try { await transaction.RollbackAsync(cleanup.Token); }
            catch { throw new InvalidOperationException("Snapshot cleanup was not confirmed."); }
            if (IsTransient(exception)) throw new LineTransientException(exception);
            throw;
        }
    }

    private Task Limits(bool readOnly, CancellationToken ct) => db.Database.ExecuteSqlRawAsync(
        readOnly ? "SET TRANSACTION READ ONLY; SET LOCAL statement_timeout = '10s'; SET LOCAL lock_timeout = '5s'"
            : "SET LOCAL statement_timeout = '10s'; SET LOCAL lock_timeout = '5s'", ct);
    private static bool IsTransient(Exception exception)
    {
        // Npgsql's non-retrying EF execution strategy can wrap a provider timeout.
        // Only an actual PostgreSQL failure in the chain proves this classification.
        for (Exception? current = exception; current is not null; current = current.InnerException)
            if (current is PostgresException { SqlState: "55P03" or "40P01" or "57014" }) return true;
        return false;
    }
    private static string Pattern(string text) => "%" + text.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("%", "\\%", StringComparison.Ordinal).Replace("_", "\\_", StringComparison.Ordinal) + "%";
    private static string Revision(long value) => value.ToString(CultureInfo.InvariantCulture);
    private static string Utc(DateTimeOffset value) => value.UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'", CultureInfo.InvariantCulture);
    private static LineProductChoice ProductChoice(Product product) => new(product.Id, product.Sku, product.Name, product.Unit, Revision(product.UnitRevision), product.IsActive);
    private static LineSummary Summary(ProductionLine line) => new(line.Id, line.Code, line.Name, LineValueRules.Format(line.WorkingHoursPerDay),
        line.IsActive, Utc(line.UpdatedAtUtc), line.RowVersion.ToString(CultureInfo.InvariantCulture));

    private sealed class WriteSession(AppDbContext db, IDbContextTransaction transaction) : ILineWriteSession
    {
        private bool finished;
        private bool disposed;
        private bool discardContext;
        private bool rollbackAttempted;
        public async Task<IReadOnlyDictionary<Guid, Product>> LockProductsAsync(IEnumerable<Guid> ids, CancellationToken ct)
        {
            var products = new Dictionary<Guid, Product>();
            foreach (var id in ids.Distinct().OrderBy(id => id.ToString("D"), StringComparer.Ordinal))
            {
                var rows = await Lock(() => db.Products.FromSqlInterpolated($"SELECT p.*, p.xmin FROM products p WHERE p.id = {id} FOR SHARE").AsNoTracking().ToListAsync(ct));
                if (rows.SingleOrDefault() is { } product) products.Add(id, product);
            }
            return products;
        }
        public async Task<ProductionLine?> LockLineAsync(Guid id, CancellationToken ct) =>
            (await Lock(() => db.ProductionLines.FromSqlInterpolated($"SELECT l.*, l.xmin FROM production_lines l WHERE l.id = {id} FOR UPDATE").ToListAsync(ct))).SingleOrDefault();
        public async Task<IReadOnlyDictionary<Guid, ProductionLineProduct>> LockPairsAsync(Guid lineId, IEnumerable<Guid> ids, CancellationToken ct)
        {
            var pairs = new Dictionary<Guid, ProductionLineProduct>();
            foreach (var id in ids.Distinct().OrderBy(id => id.ToString("D"), StringComparer.Ordinal))
            {
                var rows = await Lock(() => db.ProductionLineProducts.FromSqlInterpolated($"SELECT p.* FROM production_line_products p WHERE p.line_id = {lineId} AND p.product_id = {id} FOR UPDATE").ToListAsync(ct));
                if (rows.SingleOrDefault() is { } pair) pairs.Add(id, pair);
            }
            return pairs;
        }
        private static async Task<T> Lock<T>(Func<Task<T>> read)
        {
            try { return await read(); }
            catch (Exception exception) when (IsTransient(exception)) { throw new LineTransientException(exception); }
        }
        public void Add(ProductionLine line) => db.ProductionLines.Add(line);
        public void Add(ProductionLineProduct pair) => db.ProductionLineProducts.Add(pair);
        public void Touch(ProductionLine line, DateTimeOffset now)
        {
            line.UpdatedAtUtc = now;
            db.Entry(line).Property(l => l.UpdatedAtUtc).IsModified = true;
        }
        public async Task SaveAsync(CancellationToken ct)
        {
            try { await db.SaveChangesAsync(ct); }
            catch (DbUpdateConcurrencyException exception) { throw new LineVersionConflictException(exception); }
            catch (DbUpdateException exception) when (exception.InnerException is PostgresException { SqlState: "23505", ConstraintName: "ux_production_lines_code_lower" })
            { throw new LineCodeConflictException(exception); }
            catch (Exception exception) when (IsTransient(exception)) { throw new LineTransientException(exception); }
        }
        public async Task CommitAsync(CancellationToken ct)
        {
            await transaction.CommitAsync(ct);
            finished = true;
            await transaction.DisposeAsync();
            disposed = true;
        }
        public async Task<bool> RollbackAsync(CancellationToken ct)
        {
            if (finished) return false;
            discardContext = true;
            rollbackAttempted = true;
            await transaction.RollbackAsync(ct);
            finished = true;
            return true;
        }
        public async ValueTask DisposeAsync()
        {
            if (disposed) return;
            if (!finished && !rollbackAttempted)
            {
                rollbackAttempted = true;
                using var cleanup = new CancellationTokenSource(TimeSpan.FromSeconds(2));
                try { await transaction.RollbackAsync(cleanup.Token); } catch { /* Failed connection is discarded below. */ }
            }
            await transaction.DisposeAsync();
            disposed = true;
            if (!finished || discardContext) await db.DisposeAsync();
        }
    }
}
