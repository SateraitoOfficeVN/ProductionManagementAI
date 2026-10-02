using System.Data;
using System.Diagnostics;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using ProductionManagementAI.Application.PlantCalendar;
using ProductionManagementAI.Application.ProductionOrders;
using ProductionManagementAI.Domain.PlantCalendar;
using ProductionManagementAI.Domain.ProductionLines;
using ProductionManagementAI.Domain.ProductionOrders;

namespace ProductionManagementAI.Infrastructure.PlantCalendar;

/// <summary>Implements coherent projected reads and column-scoped retained transitions.</summary>
public sealed class CalendarRepository(AppDbContext db, IPlantClock clock, TimeProvider time) : ICalendarRepository, ICalendarReader
{
    /// <inheritdoc/>
    public async Task<T> ReadSnapshotAsync<T>(Func<ICalendarReader, CalendarContext, CancellationToken, Task<T>> read, CancellationToken ct)
    {
        using var activity = CalendarTelemetry.Source.StartActivity("plant-calendar.read");
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(ct);
        deadline.CancelAfter(TimeSpan.FromSeconds(20));
        var oldTimeout = db.Database.GetCommandTimeout();
        IDbContextTransaction? tx = null;
        try
        {
            db.Database.SetCommandTimeout(15);
            tx = await db.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead, deadline.Token);
            await db.Database.ExecuteSqlRawAsync("SET TRANSACTION READ ONLY; SET LOCAL statement_timeout = '15s'; SET LOCAL lock_timeout = '5s';", deadline.Token);
            var state = await db.CalendarStates.AsNoTracking().SingleOrDefaultAsync(deadline.Token);
            var context = Context(state, time.GetUtcNow());
            var result = await read(this, context, deadline.Token);
            await tx.CommitAsync(deadline.Token);
            return result;
        }
        catch (Exception error) when (Transient(error)) { throw Rule("CALENDAR_BUSY", 503); }
        finally
        {
            if (tx is not null) await Dispose(tx);
            db.Database.SetCommandTimeout(oldTimeout);
        }
    }

    /// <inheritdoc/>
    public async Task<MutationResult> WriteTransitionAsync(CalendarCommand command, CancellationToken ct)
    {
        using var activity = CalendarTelemetry.Source.StartActivity("plant-calendar.write");
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(ct);
        deadline.CancelAfter(TimeSpan.FromSeconds(20));
        var token = deadline.Token;
        var oldTimeout = db.Database.GetCommandTimeout();
        var oldSavepoints = db.Database.AutoSavepointsEnabled;
        IDbContextTransaction? tx = null;
        var commitIssued = false;
        CancellationTokenSource? cleanup = null;
        try
        {
            db.Database.SetCommandTimeout(15); db.Database.AutoSavepointsEnabled = false;
            tx = await db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, token);
            await db.Database.ExecuteSqlRawAsync("SET LOCAL statement_timeout = '15s'; SET LOCAL lock_timeout = '5s';", token);
            ProductionLine? line = null;
            if (command.LineId is { } lineId)
            {
                // Non-key retirement requires FOR SHARE, before the aggregate lock.
                line = await db.ProductionLines.FromSqlInterpolated($"SELECT *, xmin FROM public.production_lines WHERE id = {lineId} FOR SHARE")
                    .AsNoTracking().SingleOrDefaultAsync(token) ?? throw Rule("NOT_FOUND", 404);
            }
            var state = await db.CalendarStates.FromSqlRaw("SELECT * FROM public.plant_calendar_state WHERE id = 1 FOR UPDATE")
                .AsNoTracking().SingleOrDefaultAsync(token) ?? throw Rule("CALENDAR_NOT_ACTIVATED", 409);
            if (state.TimeZoneId != clock.TimeZoneId) throw Rule("CALENDAR_TIMEZONE_MISMATCH", 503);
            if (state.Revision != command.Version) throw Rule("CALENDAR_STALE", 409);
            var weeklyOperation = command.Kind is "SaveWeekly" or "WithdrawWeekly";
            var weekly = weeklyOperation ? await db.CalendarWeeklyRevisions.AsNoTracking()
                .SingleOrDefaultAsync(x => x.IsCurrent && x.EffectiveFrom == command.Date, token) : null;
            var exception = weeklyOperation ? null : await ExceptionAsync(command.Date, command.LineId, token);
            var currentId = weeklyOperation ? weekly?.Id : exception?.Id;
            if (currentId != command.TargetRevisionId)
            {
                if (command.TargetRevisionId is { } target)
                {
                    var exists = weeklyOperation ? await db.CalendarWeeklyRevisions.AnyAsync(x => x.Id == target, token)
                        : await db.CalendarExceptionRevisions.AnyAsync(x => x.Id == target, token);
                    if (!exists) throw Rule("NOT_FOUND", 404);
                }
                throw Rule("CALENDAR_TARGET_CHANGED", 409);
            }
            // Date is captured after every potential lock wait and reused for this accepted operation.
            var now = time.GetUtcNow();
            var context = Context(state, now);
            if (command.Date < context.PlantToday || command.Date < state.ActivatedOn) throw Rule("CALENDAR_PAST_DATE", 409);
            if (command.Kind == "WithdrawWeekly")
            {
                if (weekly is null || weekly.IsWithdrawn) throw Rule("CALENDAR_TARGET_CHANGED", 409);
                if (command.Date <= context.PlantToday || command.Date == state.ActivatedOn) throw Rule("CALENDAR_PROTECTED_RULE", 409);
            }
            if (command.Kind == "RemoveException" && (exception is null || exception.IsRemoved)) throw Rule("CALENDAR_TARGET_CHANGED", 409);
            if (command.Kind == "SaveException" && line is { IsActive: false }) throw Rule("CALENDAR_LINE_RETIRED", 409);
            var unchanged = command.Kind == "SaveWeekly" && weekly is { IsWithdrawn: false } && weekly.WorkingWeekdays == command.Mask
                || command.Kind == "SaveException" && exception is { IsRemoved: false } && exception.IsWorking == command.IsWorking
                    && exception.WorkingHours == command.Hours && exception.Reason == command.Reason;
            if (unchanged)
            {
                var target = weeklyOperation ? (object)CalendarMapping.Weekly(weekly ?? throw new InvalidOperationException())
                    : CalendarMapping.Exception(exception ?? throw new InvalidOperationException());
                commitIssued = true;
                await tx.CommitAsync(token);
                return new(context, false, target);
            }
            var next = checked(state.Revision + 1);
            object result;
            if (weeklyOperation)
            {
                if (weekly is not null && await db.CalendarWeeklyRevisions.Where(x => x.Id == weekly.Id && x.IsCurrent)
                    .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsCurrent, false), token) != 1) throw Rule("CALENDAR_TARGET_CHANGED", 409);
                var row = new WeeklyRevision { Id = Guid.CreateVersion7(), CalendarId = 1, EffectiveFrom = command.Date,
                    WorkingWeekdays = command.Kind == "WithdrawWeekly" ? null : command.Mask, IsWithdrawn = command.Kind == "WithdrawWeekly",
                    IsCurrent = true, CommitRevision = next, CreatedAtUtc = now };
                db.CalendarWeeklyRevisions.Add(row); result = CalendarMapping.Weekly(row);
            }
            else
            {
                if (exception is not null && await db.CalendarExceptionRevisions.Where(x => x.Id == exception.Id && x.IsCurrent)
                    .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsCurrent, false), token) != 1) throw Rule("CALENDAR_TARGET_CHANGED", 409);
                var removed = command.Kind == "RemoveException";
                var row = new ExceptionRevision { Id = Guid.CreateVersion7(), CalendarId = 1, LineId = command.LineId, CalendarDate = command.Date,
                    IsWorking = removed ? null : command.IsWorking, WorkingHours = removed ? null : command.Hours, Reason = removed ? null : command.Reason,
                    IsRemoved = removed, IsCurrent = true, CommitRevision = next, CreatedAtUtc = now };
                db.CalendarExceptionRevisions.Add(row); result = CalendarMapping.Exception(row);
            }
            await db.SaveChangesAsync(token);
            if (await db.CalendarStates.Where(x => x.Id == 1 && x.Revision == command.Version)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.Revision, next).SetProperty(x => x.UpdatedAtUtc, now), token) != 1)
                throw Rule("CALENDAR_STALE", 409);
            commitIssued = true; // Before COMMIT is sent: rollback afterwards alone cannot establish its outcome.
            await tx.CommitAsync(token);
            return new(context with { Version = next.ToString(CultureInfo.InvariantCulture) }, true, result);
        }
        catch (Exception error)
        {
            cleanup = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var rollback = tx is null || await Rollback(tx, cleanup.Token);
            db.ChangeTracker.Clear();
            if (commitIssued || !rollback) throw Rule("CALENDAR_WRITE_UNKNOWN", 503, "Unknown");
            if (error is CalendarRuleException rule) throw rule;
            if (Transient(error)) throw Rule("CALENDAR_BUSY", 503);
            var postgres = error as PostgresException ?? error.InnerException as PostgresException;
            if (postgres is { SqlState: "23505", ConstraintName: "ux_calendar_weekly_current" or "ux_calendar_weekly_commit" or "ux_calendar_exception_current" or "ux_calendar_exception_commit" })
                throw Rule("CALENDAR_TARGET_CHANGED", 409);
            // Generic write failures remain uncertain to clients under the approved API contract.
            throw Rule("UNEXPECTED", 500, "Unknown");
        }
        finally
        {
            if (tx is not null)
            {
                cleanup ??= new CancellationTokenSource(TimeSpan.FromSeconds(5));
                await Dispose(tx, cleanup.Token);
            }
            cleanup?.Dispose();
            db.Database.SetCommandTimeout(oldTimeout); db.Database.AutoSavepointsEnabled = oldSavepoints;
        }
    }

    private CalendarContext Context(CalendarState? state, DateTimeOffset now)
    {
        if (state is not null && state.TimeZoneId != clock.TimeZoneId) throw Rule("CALENDAR_TIMEZONE_MISMATCH", 503);
        return new(clock.DateOf(now), clock.TimeZoneId, state?.ActivatedOn, state?.Revision.ToString(CultureInfo.InvariantCulture));
    }
    private static CalendarRuleException Rule(string code, int status, string outcome = "NotApplied") => new(new(code, status, outcome));
    private static bool Transient(Exception e) => e is OperationCanceledException or TimeoutException
        || (e as PostgresException ?? e.InnerException as PostgresException)?.SqlState is "55P03" or "57014" or "40P01";
    private static async Task<bool> Rollback(IDbContextTransaction tx, CancellationToken ct)
    {
        try { await tx.RollbackAsync(ct); return true; } catch (Exception) { return false; }
    }
    private static async Task Dispose(IDbContextTransaction tx, CancellationToken? ct = null)
    {
        using var timeout = ct is null ? new CancellationTokenSource(TimeSpan.FromSeconds(5)) : null;
        try { await tx.DisposeAsync().AsTask().WaitAsync(ct ?? timeout!.Token); } catch (Exception) { /* Known commit is not undone by cleanup errors. */ }
    }

    /// <inheritdoc/>
    public Task<ProductionLine?> LineAsync(Guid id, CancellationToken ct) => db.ProductionLines.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
    /// <inheritdoc/>
    public Task<Product?> ProductAsync(Guid id, CancellationToken ct) => db.Products.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
    /// <inheritdoc/>
    public Task<ProductionLineProduct?> PairAsync(Guid lineId, Guid productId, CancellationToken ct) => db.ProductionLineProducts.AsNoTracking().SingleOrDefaultAsync(x => x.LineId == lineId && x.ProductId == productId, ct);
    /// <inheritdoc/>
    public Task<WeeklyRevision?> ApplicableWeeklyAsync(DateOnly date, CancellationToken ct) => db.CalendarWeeklyRevisions.AsNoTracking()
        .Where(x => x.IsCurrent && !x.IsWithdrawn && x.EffectiveFrom <= date).OrderByDescending(x => x.EffectiveFrom).FirstOrDefaultAsync(ct);
    /// <inheritdoc/>
    public async Task<IReadOnlyList<WeeklyRevision>> MonthWeeklyAsync(DateOnly from, DateOnly to, CancellationToken ct) => await db.CalendarWeeklyRevisions.AsNoTracking()
        .Where(x => x.IsCurrent && !x.IsWithdrawn && x.EffectiveFrom >= from && x.EffectiveFrom <= to).OrderBy(x => x.EffectiveFrom).ToListAsync(ct);
    /// <inheritdoc/>
    public async Task<IReadOnlyList<ExceptionRevision>> MonthExceptionsAsync(DateOnly from, DateOnly to, Guid? lineId, CancellationToken ct) => await db.CalendarExceptionRevisions.AsNoTracking()
        .Where(x => x.IsCurrent && x.CalendarDate >= from && x.CalendarDate <= to && (x.LineId == null || x.LineId == lineId)).ToListAsync(ct);
    /// <inheritdoc/>
    public Task<ExceptionRevision?> ExceptionAsync(DateOnly date, Guid? lineId, CancellationToken ct) => db.CalendarExceptionRevisions.AsNoTracking()
        .SingleOrDefaultAsync(x => x.IsCurrent && x.CalendarDate == date && x.LineId == lineId, ct);
    /// <inheritdoc/>
    public async Task<RecordPage<WeeklyRevision>> WeeklyPageAsync(CalendarReadQuery q, CancellationToken ct)
    {
        var rows = db.CalendarWeeklyRevisions.AsNoTracking().Where(x => x.EffectiveFrom >= q.From && x.EffectiveFrom <= q.To);
        if (q.View == "Current") rows = rows.Where(x => x.IsCurrent);
        var total = await rows.CountAsync(ct);
        return new(await rows.OrderByDescending(x => x.EffectiveFrom).ThenByDescending(x => x.CommitRevision).ThenBy(x => x.Id).Skip((q.Page - 1) * 20).Take(20).ToListAsync(ct), total);
    }
    /// <inheritdoc/>
    public async Task<RecordPage<ExceptionRevision>> ExceptionPageAsync(CalendarReadQuery q, CancellationToken ct)
    {
        var rows = db.CalendarExceptionRevisions.AsNoTracking().Where(x => x.CalendarDate == q.Date && x.LineId == q.LineId);
        var total = await rows.CountAsync(ct);
        return new(await rows.OrderByDescending(x => x.CommitRevision).ThenBy(x => x.Id).Skip((q.Page - 1) * 20).Take(20).ToListAsync(ct), total);
    }
    /// <inheritdoc/>
    public async Task<RecordPage<ProductionLine>> LineChoicesAsync(CalendarReadQuery q, CancellationToken ct)
    {
        var rows = db.ProductionLines.AsNoTracking().Where(x => x.IsActive);
        if (q.Search is { } search) { var pattern = Pattern(search); rows = rows.Where(x => EF.Functions.ILike(x.Code, pattern, "\\") || EF.Functions.ILike(x.Name, pattern, "\\")); }
        var total = await rows.CountAsync(ct);
        return new(await rows.OrderBy(x => x.Code).ThenBy(x => x.Id).Skip((q.Page - 1) * 50).Take(50).ToListAsync(ct), total);
    }
    /// <inheritdoc/>
    public async Task<RecordPage<Product>> ProductChoicesAsync(CalendarReadQuery q, CancellationToken ct)
    {
        var rows = from product in db.Products.AsNoTracking()
                   join pair in db.ProductionLineProducts.AsNoTracking() on product.Id equals pair.ProductId
                   join line in db.ProductionLines.AsNoTracking() on pair.LineId equals line.Id
                   where line.Id == q.LineId && line.IsActive && product.IsActive && pair.IsActive
                       && pair.ConfirmedUnit == product.Unit && pair.ConfirmedUnitRevision == product.UnitRevision
                   select product;
        if (q.Search is { } search) { var pattern = Pattern(search); rows = rows.Where(x => EF.Functions.ILike(x.Sku, pattern, "\\") || EF.Functions.ILike(x.Name, pattern, "\\")); }
        var total = await rows.CountAsync(ct);
        return new(await rows.OrderBy(x => x.Sku).ThenBy(x => x.Id).Skip((q.Page - 1) * 50).Take(50).ToListAsync(ct), total);
    }
    private static string Pattern(string text) => "%" + text.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("%", "\\%", StringComparison.Ordinal).Replace("_", "\\_", StringComparison.Ordinal) + "%";
}
