using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging;
using ProductionManagementAI.Domain.ProductionLines;
using ProductionManagementAI.Domain.ProductionOrders;

namespace ProductionManagementAI.Application.ProductionLines;

/// <summary>Executes bounded production-line use cases without automatically replaying writes.</summary>
public sealed class ProductionLineService(IProductionLineRepository repository, TimeProvider timeProvider, ILogger<ProductionLineService>? logger = null)
{
    /// <summary>Gets the feature ActivitySource and Meter registration name.</summary>
    public const string TelemetryName = "ProductionManagementAI.ProductionLines";
    private static readonly ActivitySource Source = new(TelemetryName);
    private static readonly Meter Meter = new(TelemetryName);
    private static readonly Counter<long> Requests = Meter.CreateCounter<long>("pmai.production_lines.requests");
    private static readonly Histogram<double> Duration = Meter.CreateHistogram<double>("pmai.production_lines.duration", "s");

    /// <summary>Reads a validated line page from one snapshot.</summary>
    public Task<LineResult<LinePage<LineSummary>>> ListAsync(LineListRequest request, CancellationToken cancellationToken) =>
        Execute("list", "List", async ct =>
        {
            var parsed = LineValidation.Query(request.Q, request.Page, request.State, pageSize: request.PageSize);
            return parsed.Value is { } query
                ? LineResult<LinePage<LineSummary>>.Success(await repository.ListAsync(query, ct))
                : new(default, parsed.Problem);
        }, cancellationToken);

    /// <summary>Reads a validated line detail and association page.</summary>
    public Task<LineResult<LineDetail>> GetAsync(Guid id, string? pairsPage, CancellationToken cancellationToken, string? pairsPageSize = null) =>
        Execute("get", "Get", async ct =>
        {
            var parsed = LineValidation.Query(null, pairsPage, pageSize: pairsPageSize, pageSizeField: "pairsPageSize");
            if (id == Guid.Empty) return LineResult<LineDetail>.Failure(400, "VALIDATION", "id");
            if (parsed.Value is not { } query)
                return LineResult<LineDetail>.Failure(400, "VALIDATION",
                    parsed.Problem?.Errors?.ContainsKey("pairsPageSize") == true ? "pairsPageSize" : "pairsPage");
            var value = await repository.GetAsync(id, query.Page, query.PageSize, ct);
            return value is null ? LineResult<LineDetail>.Failure(404, "NOT_FOUND") : LineResult<LineDetail>.Success(value);
        }, cancellationToken);

    /// <summary>Reads product choices while retaining persisted retired associations.</summary>
    public Task<LineResult<LinePage<LineProductChoice>>> ProductChoicesAsync(string? q, string? page, Guid? lineId, CancellationToken cancellationToken, string? pageSize = null) =>
        Execute("product_choices", "ProductChoices", async ct =>
        {
            var parsed = LineValidation.Query(q, page, targetId: lineId, pageSize: pageSize);
            if (parsed.Value is not { } query) return new(default, parsed.Problem);
            var value = await repository.ProductChoicesAsync(query, ct);
            return value is null ? LineResult<LinePage<LineProductChoice>>.Failure(404, "NOT_FOUND") : LineResult<LinePage<LineProductChoice>>.Success(value);
        }, cancellationToken);

    /// <summary>Reads eligible lines for a current product observation.</summary>
    public Task<LineResult<EligibleLinePage>> EligibleAsync(Guid productId, string? q, string? page, CancellationToken cancellationToken) =>
        Execute("eligible", "Eligible", async ct =>
        {
            if (productId == Guid.Empty) return LineResult<EligibleLinePage>.Failure(400, "VALIDATION", "productId");
            var parsed = LineValidation.Query(q, page, targetId: productId);
            if (parsed.Value is not { } query) return new(default, parsed.Problem);
            var value = await repository.EligibleAsync(query, ct);
            return value is null ? LineResult<EligibleLinePage>.Failure(404, "NOT_FOUND") : LineResult<EligibleLinePage>.Success(value);
        }, cancellationToken);

    /// <summary>Creates a line and explicitly confirmed associations atomically.</summary>
    public Task<LineResult<LineDetail>> CreateAsync(CreateLineRequest request, CancellationToken cancellationToken) =>
        Execute("create", "Create", async ct =>
        {
            var parsed = LineValidation.Create(request);
            if (parsed.Value is not { } command) return new(default, parsed.Problem);
            var now = timeProvider.GetUtcNow();
            var id = Guid.CreateVersion7(now);
            return await Write(id, command, true, ct);
        }, cancellationToken);

    /// <summary>Updates mutable line fields and submitted association intents as one aggregate.</summary>
    public Task<LineResult<LineDetail>> UpdateAsync(Guid id, UpdateLineRequest request, CancellationToken cancellationToken) =>
        Execute("update", "Update", async ct =>
        {
            if (id == Guid.Empty) return LineResult<LineDetail>.Failure(400, "VALIDATION", "id");
            var parsed = LineValidation.Update(request);
            return parsed.Value is { } command ? await Write(id, command, false, ct) : new(default, parsed.Problem);
        }, cancellationToken);

    /// <summary>Retires a versioned line without altering pairs or historical orders.</summary>
    public Task<LineResult<LineSummary>> RetireAsync(Guid id, string? version, CancellationToken cancellationToken) =>
        Execute("retire", "Retire", async ct =>
        {
            if (id == Guid.Empty) return LineResult<LineSummary>.Failure(400, "VALIDATION", "id");
            if (!LineValueRules.TryVersion(version, out var expected)) return LineResult<LineSummary>.Failure(400, "VALIDATION", "version");
            await using var session = await repository.BeginWriteAsync(ct);
            var commitStarted = false;
            try
            {
                var line = await session.LockLineAsync(id, ct);
                if (line is null) return LineResult<LineSummary>.Failure(404, "NOT_FOUND");
                if (line.RowVersion != expected) return LineResult<LineSummary>.Failure(409, "LINE_STALE", "version");
                if (!line.IsActive) return LineResult<LineSummary>.Failure(400, "LINE_ALREADY_RETIRED");
                line.IsActive = false;
                session.Touch(line, timeProvider.GetUtcNow());
                await session.SaveAsync(ct);
                commitStarted = true;
                await session.CommitAsync(ct);
                var saved = await repository.GetAsync(id, 1, LineValidation.DefaultPageSize, ct);
                return saved is null ? LineResult<LineSummary>.Failure(500, "UNEXPECTED") : LineResult<LineSummary>.Success(
                    new(saved.Id, saved.Code, saved.Name, saved.WorkingHoursPerDay, saved.IsActive, saved.UpdatedAt, saved.Version));
            }
            catch (Exception exception)
            {
                return new(default, await ClassifyWriteFailure(session, exception, commitStarted));
            }
        }, cancellationToken);

    private async Task<LineResult<LineDetail>> Write(Guid id, LineCommand command, bool create, CancellationToken ct)
    {
        await using var session = await repository.BeginWriteAsync(ct);
        var commitStarted = false;
        try
        {
            Activity.Current?.AddEvent(new ActivityEvent("lock.products"));
            var products = await session.LockProductsAsync(command.Changes.Select(c => c.ProductId), ct);
            Activity.Current?.AddEvent(new ActivityEvent("lock.parent"));
            var line = create ? null : await session.LockLineAsync(id, ct);
            if (!create && line is null) return LineResult<LineDetail>.Failure(404, "NOT_FOUND");
            if (line is not null && line.RowVersion != command.Version) return LineResult<LineDetail>.Failure(409, "LINE_STALE", "version");
            Activity.Current?.AddEvent(new ActivityEvent("lock.pairs"));
            var pairs = create ? new Dictionary<Guid, ProductionLineProduct>()
                : await session.LockPairsAsync(id, command.Changes.Select(c => c.ProductId), ct);
            foreach (var change in command.Changes)
            {
                pairs.TryGetValue(change.ProductId, out var pair);
                if (change.Action == "add" && pair is not null)
                    return LineResult<LineDetail>.Failure(400, "LINE_PAIR_INVALID", $"{change.Field}.productId");
                if (change.Action != "add" && pair is null) return LineResult<LineDetail>.Failure(404, "NOT_FOUND");
                if (pair is not null && !pair.IsActive)
                    return LineResult<LineDetail>.Failure(400, "LINE_PAIR_INVALID", $"{change.Field}.action");
                if (change.Action == "retire") continue;
                if (!products.TryGetValue(change.ProductId, out var product) || (change.Action == "add" && !product.IsActive))
                    return LineResult<LineDetail>.Failure(400, "LINE_PRODUCT_INVALID", $"{change.Field}.productId");
                if (product.Unit != change.ExpectedUnit || product.UnitRevision != change.ExpectedUnitRevision)
                    return LineResult<LineDetail>.Failure(409, "LINE_UNIT_STALE", $"{change.Field}.expectedUnitRevision");
                if (pair is not null && !change.ConfirmUnit && (pair.ConfirmedUnit != product.Unit || pair.ConfirmedUnitRevision != product.UnitRevision))
                    return LineResult<LineDetail>.Failure(409, "LINE_UNIT_CONFIRMATION_REQUIRED", $"{change.Field}.confirmUnit");
            }
            Activity.Current?.AddEvent(new ActivityEvent("validation.accepted"));
            var now = timeProvider.GetUtcNow();
            if (line is null)
            {
                line = new ProductionLine { Id = id, Code = command.Code, Name = command.Name,
                    WorkingHoursPerDay = command.WorkingHoursPerDay, CreatedAtUtc = now, UpdatedAtUtc = now };
                session.Add(line);
            }
            else
            {
                line.Name = command.Name;
                line.WorkingHoursPerDay = command.WorkingHoursPerDay;
                session.Touch(line, now);
            }
            foreach (var change in command.Changes)
            {
                if (change.Action == "add") session.Add(new ProductionLineProduct {
                    LineId = id, ProductId = change.ProductId, MinutesPerUnit = change.MinutesPerUnit,
                    ConfirmedUnit = change.ExpectedUnit, ConfirmedUnitRevision = change.ExpectedUnitRevision,
                    CreatedAtUtc = now, UpdatedAtUtc = now });
                else if (pairs.TryGetValue(change.ProductId, out var pair))
                {
                    if (change.Action == "retire") pair.IsActive = false;
                    else
                    {
                        pair.MinutesPerUnit = change.MinutesPerUnit;
                        if (change.ConfirmUnit) { pair.ConfirmedUnit = change.ExpectedUnit; pair.ConfirmedUnitRevision = change.ExpectedUnitRevision; }
                    }
                    pair.UpdatedAtUtc = now;
                }
            }
            await session.SaveAsync(ct);
            commitStarted = true;
            await session.CommitAsync(ct);
            var saved = await repository.GetAsync(id, 1, LineValidation.DefaultPageSize, ct);
            return saved is null ? LineResult<LineDetail>.Failure(500, "UNEXPECTED") : LineResult<LineDetail>.Success(saved);
        }
        catch (Exception exception)
        {
            return new(default, await ClassifyWriteFailure(session, exception, commitStarted));
        }
    }

    private static async Task<LineProblem> ClassifyWriteFailure(ILineWriteSession session, Exception exception, bool commitStarted)
    {
        using var cleanup = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        var rolledBack = false;
        try { rolledBack = await session.RollbackAsync(cleanup.Token); } catch { /* Classification remains unknown. */ }
        if (commitStarted) return new(500, "UNEXPECTED");
        if (exception is LineCodeConflictException && rolledBack)
            return new(409, "LINE_CODE_CONFLICT", new Dictionary<string, string[]> { ["code"] = ["LINE_CODE_CONFLICT"] });
        if (exception is LineVersionConflictException && rolledBack)
            return new(409, "LINE_STALE", new Dictionary<string, string[]> { ["version"] = ["LINE_STALE"] });
        return exception is LineTransientException && rolledBack ? new(503, "LINE_BUSY") : new(500, "UNEXPECTED");
    }

    /// <summary>Records one rejected feature-boundary request using bounded operation and outcome values.</summary>
    /// <param name="operation">The catalog operation name.</param>
    /// <param name="outcome">The classified boundary outcome.</param>
    /// <param name="seconds">The elapsed request duration.</param>
    public static void RecordBoundary(string operation, string outcome, double seconds)
    {
        var tags = new TagList { { "operation", operation }, { "outcome", outcome } };
        Requests.Add(1, tags);
        Duration.Record(seconds, tags);
    }

    private async Task<LineResult<T>> Execute<T>(string operation, string span, Func<CancellationToken, Task<LineResult<T>>> action, CancellationToken cancellationToken)
    {
        using var activity = Source.StartActivity($"ProductionLine.{span}");
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        deadline.CancelAfter(TimeSpan.FromSeconds(15));
        var started = Stopwatch.GetTimestamp();
        var outcome = "error";
        try
        {
            LineResult<T> result;
            try { result = await action(deadline.Token); }
            catch (LineTransientException) { result = LineResult<T>.Failure(503, "LINE_BUSY"); }
            catch (OperationCanceledException) { outcome = "cancelled"; return LineResult<T>.Failure(500, "UNEXPECTED"); }
            catch { return LineResult<T>.Failure(500, "UNEXPECTED"); }
            outcome = deadline.IsCancellationRequested && result.Problem?.Status == 500 ? "cancelled" : result.Problem?.Status switch { null => "success", 400 => "validation", 404 => "not_found", 409 => "conflict", 503 => "busy", _ => "unknown" };
            return result;
        }
        finally
        {
            var tags = new TagList { { "operation", operation }, { "outcome", outcome } };
            Requests.Add(1, tags);
            Duration.Record(Stopwatch.GetElapsedTime(started).TotalSeconds, tags);
            activity?.SetTag("operation", operation);
            activity?.SetTag("outcome", outcome);
            if (outcome is "error" or "unknown")
            {
                activity?.SetStatus(ActivityStatusCode.Error);
                logger?.LogError("Production line {Operation} finished with {Outcome}; trace {TraceId}", operation, outcome, activity?.Id);
            }
        }
    }
}
