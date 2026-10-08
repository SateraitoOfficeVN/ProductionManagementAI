using System.Diagnostics;
using Microsoft.Extensions.Logging;
using ProductionManagementAI.Domain.ProductionOrders;
using static ProductionManagementAI.Application.ProductionOrders.ProductionOrderTelemetry;
using Msg = ProductionManagementAI.Domain.ProductionOrders.ProductionOrderMessages;

namespace ProductionManagementAI.Application.ProductionOrders;

/// <summary>
/// Production-order use cases (001_DD-FN). Checks run in a fixed order so each failure has exactly one outcome:
/// shape (400) → existence (404) → version (409) → Domain rules (422) → data-dependent checks (400) → save race (409).
/// </summary>
public sealed partial class ProductionOrderService(
    IProductionOrderRepository repository,
    IOrderNumberIssuer orderNumberIssuer,
    IPlantClock plantClock,
    TimeProvider timeProvider,
    ILogger<ProductionOrderService> logger)
{
    public Task<IReadOnlyList<ProductResponse>> ListProductsAsync(CancellationToken cancellationToken) =>
        repository.ListProductsAsync(cancellationToken);

    public async Task<Result<ProductionOrderResponse>> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var order = await repository.FindAsync(id, tracked: false, cancellationToken);
        if (order is null)
        {
            return new Result<ProductionOrderResponse>.NotFound();
        }

        var product = await repository.FindProductAsync(order.ProductId, cancellationToken)
            ?? throw new InvalidOperationException("An order references a missing product.");
        return new Result<ProductionOrderResponse>.Ok(ProductionOrderMapper.ToResponse(order, product.Unit, await ReadLine(order.LineId, cancellationToken)));
    }

    /// <summary>
    /// Prepares a CSV export for an already-validated query (002_DD-FN-CSV §1). The count and the rows come from one
    /// read-only snapshot, so <see cref="ProductionOrderExport.Count"/> always equals the rows written; no row is read
    /// before the row limit passes. On <c>Ok</c> the caller owns the export (snapshot and activity) and must dispose it.
    /// </summary>
    public async Task<Result<ProductionOrderExport>> ExportAsync(
        ProductionOrderListQuery query, string? userId, CancellationToken cancellationToken)
    {
        var activity = Source.StartActivity("ProductionOrder.Export");
        SetFilterTags(activity, query);
        IProductionOrderTransaction? snapshot = null;
        try
        {
            if (query.ProductId is { } productId && !await repository.ProductExistsAsync(productId, cancellationToken))
            {
                EndExport(activity, Outcomes.ValidationFailed, userId);
                return new Result<ProductionOrderExport>.Invalid(
                    new Dictionary<string, string[]>(StringComparer.Ordinal) { ["productId"] = [Msg.ProductNotFound] });
            }

            snapshot = await repository.BeginReadSnapshotAsync(cancellationToken);
            var count = await repository.CountOrdersAsync(query, cancellationToken);
            activity?.SetTag("result.total", count);
            if (count > ProductionOrderExport.MaxRows)
            {
                await snapshot.DisposeAsync();
                EndExport(activity, Outcomes.RuleViolation, userId);
                return new Result<ProductionOrderExport>.RuleViolation(Msg.ExportLimitExceeded);
            }

            // Read once, so the file name, the overdue column and every timestamp agree (002_DD-FN-CSV §1 step 6).
            var utcNow = timeProvider.GetUtcNow();
            var export = new ProductionOrderExport(
                count,
                plantClock.ToPlantTime(utcNow),
                plantClock.DateOf(utcNow),
                plantClock.ToPlantTime,
                repository.StreamExportRowsAsync(query),
                snapshot,
                activity,
                logger,
                userId);
            return new Result<ProductionOrderExport>.Ok(export);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            if (snapshot is not null)
            {
                await snapshot.DisposeAsync();
            }

            LogUnexpectedFailure(logger, ex, "export");
            EndExport(activity, Outcomes.Error, userId);
            throw;
        }
        catch (OperationCanceledException)
        {
            if (snapshot is not null)
            {
                await snapshot.DisposeAsync();
            }

            EndExport(activity, Outcomes.Cancelled, userId);
            throw;
        }
    }

    /// <summary>Ends an export that never reached streaming: counter, activity and log 2005, then the activity.</summary>
    private void EndExport(Activity? activity, string outcome, string? userId)
    {
        Complete(activity, Exported, outcome);
        ProductionOrderExportLog.Exported(logger, userId, outcome, 0, 0);
        activity?.Dispose();
    }

    /// <summary>
    /// One counted page of production orders for an already-validated query (002_DD-FN §1). The count runs first, so a
    /// page past the last one costs one query instead of two and the total is always available for the summary.
    /// </summary>
    public async Task<Result<PagedResult<ProductionOrderListItem>>> ListAsync(
        ProductionOrderListQuery query, CancellationToken cancellationToken)
    {
        using var activity = Source.StartActivity("ProductionOrder.List");
        SetQueryTags(activity, query);
        try
        {
            if (query.ProductId is { } productId && !await repository.ProductExistsAsync(productId, cancellationToken))
            {
                Complete(activity, Listed, Outcomes.ValidationFailed);
                LogValidationFailed(logger, orderId: null, "productId");
                return new Result<PagedResult<ProductionOrderListItem>>.Invalid(
                    new Dictionary<string, string[]>(StringComparer.Ordinal) { ["productId"] = [Msg.ProductNotFound] });
            }

            var total = await repository.CountOrdersAsync(query, cancellationToken);
            IReadOnlyList<ProductionOrderListRow> rows = total == 0
                ? []
                : await repository.ListOrdersAsync(query, cancellationToken);

            var plantToday = plantClock.Today;
            var items = new List<ProductionOrderListItem>(rows.Count);
            foreach (var row in rows)
            {
                items.Add(ProductionOrderListMapper.ToListItem(row, plantToday));
            }

            activity?.SetTag("result.total", total);
            ListResultSize.Record(items.Count);
            Complete(activity, Listed, Outcomes.Success);
            return new Result<PagedResult<ProductionOrderListItem>>.Ok(new PagedResult<ProductionOrderListItem>(
                items, total, query.Page, query.PageSize, query.Sort.ToApiValue(), query.Direction.ToApiValue()));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Complete(activity, Listed, Outcomes.Error);
            LogUnexpectedFailure(logger, ex, "list");
            throw;
        }
    }

    public async Task<Result<ProductionOrderResponse>> CreateAsync(
        CreateProductionOrderRequest request, CancellationToken cancellationToken)
    {
        using var activity = Source.StartActivity("ProductionOrder.Create");
        try
        {
            var errors = new FieldErrors();
            ValidateCommonFields(errors, request.ProductId, request.Quantity, request.DueDate, request.Notes);

            if (request.DueDate is { } dueDate && dueDate < plantClock.Today)
            {
                errors.Add("dueDate", Msg.DueDateInPast);
            }

            if (errors.Any || request is not { ProductId: { } validProductId, Quantity: { } quantity, DueDate: { } validDueDate })
            {
                return Invalid(activity, Created, errors, orderId: null);
            }

            await using var transaction = await repository.BeginTransactionAsync(cancellationToken);
            var product = await repository.LockProductAsync(validProductId, cancellationToken);
            if (product is null)
            {
                RecordUnitValidation(activity, "not_found");
                errors.Add("productId", Msg.ProductNotFound);
                return Invalid(activity, Created, errors, orderId: null);
            }
            if (!product.IsActive)
            {
                RecordUnitValidation(activity, "inactive");
                Complete(activity, Created, Outcomes.ValidationFailed);
                return ProductInactive(validProductId);
            }
            if (!QuantityMatchesUnit(quantity, product.Unit))
            {
                RecordUnitValidation(activity, "quantity_invalid");
                errors.Add("quantity", Msg.QuantityUnitInvalid);
                return Invalid(activity, Created, errors, orderId: null);
            }

            if (request.LineId is { } selectedLine && !await ValidateLine(validProductId, selectedLine, cancellationToken))
                return LineFailure(activity, Created, "LINE_INELIGIBLE");

            var year = plantClock.CurrentYear;
            var sequence = await orderNumberIssuer.NextAsync(year, cancellationToken);
            RecordUnitValidation(activity, "valid");
            var order = ProductionOrder.Create(
                validProductId, quantity, validDueDate, request.Notes, year, sequence, timeProvider.GetUtcNow());

            order.AssignLine(request.LineId);
            repository.Add(order);
            await repository.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            activity?.SetTag("production_order.id", order.Id);
            activity?.SetTag("production_order.number", order.OrderNumber);
            Complete(activity, Created, Outcomes.Success);
            return new Result<ProductionOrderResponse>.Ok(ProductionOrderMapper.ToResponse(order, product.Unit, await ReadLine(order.LineId, cancellationToken)));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Complete(activity, Created, Outcomes.Error);
            LogUnexpectedFailure(logger, ex, "create");
            throw;
        }
    }

    public async Task<Result<ProductionOrderResponse>> UpdateAsync(
        Guid id, UpdateProductionOrderRequest request, CancellationToken cancellationToken)
    {
        using var activity = Source.StartActivity("ProductionOrder.Update");
        activity?.SetTag("production_order.id", id);
        try
        {
            var errors = new FieldErrors();
            ValidateCommonFields(errors, request.ProductId, request.Quantity, request.DueDate, request.Notes);
            if (request.Status is null)
            {
                errors.Add("status", Msg.StatusTransitionNotAllowed);
            }

            if (request.Version is null)
            {
                errors.Add("version", Msg.ConcurrencyConflict);
            }

            if (errors.Any || request is not
                {
                    ProductId: { } productId, Quantity: { } quantity, DueDate: { } dueDate,
                    Status: { } status, Version: { } version,
                })
            {
                return Invalid(activity, Updated, errors, id);
            }

            await using var transaction = await repository.BeginTransactionAsync(cancellationToken);
            var order = await repository.FindAsync(id, tracked: true, cancellationToken);
            if (order is null)
            {
                Complete(activity, Updated, Outcomes.NotFound);
                return new Result<ProductionOrderResponse>.NotFound();
            }

            if (order.RowVersion != version)
            {
                return Conflict(activity, id);
            }

            var fromStatus = order.Status;
            var productChanged = productId != order.ProductId;
            var dueDateChanged = dueDate != order.DueDate;
            ProductResponse? selectedProduct = null;
            if (order.IsProductQuantityEditable)
            {
                selectedProduct = await repository.LockProductAsync(productId, cancellationToken);
                if (selectedProduct is null)
                {
                    RecordUnitValidation(activity, "not_found");
                    errors.Add("productId", Msg.ProductNotFound);
                    return Invalid(activity, Updated, errors, id);
                }
                if (productChanged && !selectedProduct.IsActive)
                {
                    RecordUnitValidation(activity, "inactive");
                    Complete(activity, Updated, Outcomes.ValidationFailed);
                    return ProductInactive(productId);
                }
                if (!QuantityMatchesUnit(quantity, selectedProduct.Unit))
                {
                    RecordUnitValidation(activity, "quantity_invalid");
                    errors.Add("quantity", Msg.QuantityUnitInvalid);
                    return Invalid(activity, Updated, errors, id);
                }
            }
            RecordUnitValidation(activity, "valid");
            activity?.SetTag("status.from", fromStatus.ToString());
            activity?.SetTag("status.to", status.ToString());

            try
            {
                order.ValidateUpdate(productId, quantity, request.Notes, status);
            }
            catch (DomainRuleViolation violation)
            {
                Complete(activity, Updated, Outcomes.RuleViolation);
                LogRuleViolated(logger, id, violation.Code, fromStatus, status);
                return new Result<ProductionOrderResponse>.RuleViolation(violation.Code);
            }

            var requestedLine = request.HasLineId ? request.LineId : order.LineId;
            if (fromStatus != ProductionOrderStatus.Draft && requestedLine != order.LineId)
                return LineFailure(activity, Updated, "LINE_LOCKED");
            var starting = fromStatus == ProductionOrderStatus.Draft && status == ProductionOrderStatus.InProgress;
            if (starting && requestedLine is null) return LineFailure(activity, Updated, "LINE_REQUIRED");
            if (requestedLine is { } selectedLine && (starting || productChanged || requestedLine != order.LineId) &&
                !await ValidateLine(productId, selectedLine, cancellationToken))
                return LineFailure(activity, Updated, "LINE_INELIGIBLE");
            // Data-dependent checks, only for values the user actually changed (DEC-009). On failure nothing is saved:
            // the tracked changes are discarded with the request-scoped context.
            if (dueDateChanged && dueDate < plantClock.Today)
            {
                errors.Add("dueDate", Msg.DueDateInPast);
            }

            if (errors.Any)
            {
                return Invalid(activity, Updated, errors, id);
            }

            order.AssignLine(requestedLine);
            order.Update(productId, quantity, dueDate, request.Notes, status, timeProvider.GetUtcNow());
            try
            {
                await repository.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (ConcurrencyConflictException)
            {
                return Conflict(activity, id);
            }

            if (order.Status != fromStatus)
            {
                StatusTransitions.Add(1,
                    new KeyValuePair<string, object?>("from", fromStatus.ToString()),
                    new KeyValuePair<string, object?>("to", order.Status.ToString()));
            }

            Complete(activity, Updated, Outcomes.Success);
            selectedProduct ??= await repository.FindProductAsync(order.ProductId, cancellationToken)
                ?? throw new InvalidOperationException("An order references a missing product.");
            return new Result<ProductionOrderResponse>.Ok(ProductionOrderMapper.ToResponse(order, selectedProduct.Unit, await ReadLine(order.LineId, cancellationToken)));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Complete(activity, Updated, Outcomes.Error);
            LogUnexpectedFailure(logger, ex, "update");
            throw;
        }
    }

    private async Task<OrderLineResponse?> ReadLine(Guid? id, CancellationToken ct) => id is { } lineId
        ? await repository.FindLineAsync(lineId, ct) ?? throw new InvalidOperationException("An order references a missing line.") : null;

    private async Task<bool> ValidateLine(Guid productId, Guid lineId, CancellationToken ct)
    {
        using var activity = LineValidationSource.StartActivity("ProductionLine.ValidateOrder");
        var eligible = lineId != Guid.Empty && await repository.LockEligibleLineAsync(productId, lineId, ct);
        activity?.SetTag("outcome", eligible ? "success" : "validation");
        return eligible;
    }
    private static readonly ActivitySource LineValidationSource = new(ProductionLines.ProductionLineService.TelemetryName);
    private static Result<ProductionOrderResponse> LineFailure(Activity? activity, System.Diagnostics.Metrics.Counter<long> counter, string code)
    {
        Complete(activity, counter, Outcomes.ValidationFailed);
        return new Result<ProductionOrderResponse>.FieldProblem(code, new Dictionary<string, string[]> { ["lineId"] = [code] });
    }

    private static void ValidateCommonFields(
        FieldErrors errors, Guid? productId, decimal? quantity, DateOnly? dueDate, string? notes)
    {
        if (productId is null)
        {
            errors.Add("productId", Msg.ProductRequired);
        }

        if (quantity is null)
        {
            errors.Add("quantity", Msg.QuantityInvalid);
        }
        else if (ProductionOrder.ValidateQuantity(quantity.Value) is { } quantityError)
        {
            errors.Add("quantity", quantityError);
        }

        if (dueDate is null)
        {
            errors.Add("dueDate", Msg.DueDateRequired);
        }

        if (ProductionOrder.ValidateNotes(notes) is { } notesError)
        {
            errors.Add("notes", notesError);
        }
    }

    private Result<ProductionOrderResponse> Invalid(
        Activity? activity, System.Diagnostics.Metrics.Counter<long> counter, FieldErrors errors, Guid? orderId)
    {
        Complete(activity, counter, Outcomes.ValidationFailed);
        // Field names only — never values (notes are free text).
        LogValidationFailed(logger, orderId, string.Join(",", errors.FieldNames));
        return errors.Has("quantity")
            ? new Result<ProductionOrderResponse>.FieldProblem("QUANTITY_UNIT_INVALID", errors.ToDictionary())
            : new Result<ProductionOrderResponse>.Invalid(errors.ToDictionary());
    }

    private static bool QuantityMatchesUnit(decimal quantity, string unit) =>
        unit is "kg" or "m" || quantity == decimal.Truncate(quantity);

    private static Result<ProductionOrderResponse> ProductInactive(Guid productId) =>
        new Result<ProductionOrderResponse>.FieldProblem("PRODUCT_INACTIVE",
            new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                ["productId"] = [Msg.ProductInactive],
            });

    private Result<ProductionOrderResponse> Conflict(Activity? activity, Guid id)
    {
        Complete(activity, Updated, Outcomes.Conflict);
        LogConcurrencyConflict(logger, id);
        return new Result<ProductionOrderResponse>.Conflict();
    }

    /// <summary>Filter shape only — never the order-number fragment, which is user-supplied text (002_DD-FN).</summary>
    private static void SetQueryTags(Activity? activity, ProductionOrderListQuery query)
    {
        if (activity is null)
        {
            return;
        }

        SetFilterTags(activity, query);
        activity.SetTag("page", query.Page);
        activity.SetTag("page_size", query.PageSize);
    }

    /// <summary>The list's filter and sort tags without paging, shared by the export (002_DD-FN-CSV §1 step 1).</summary>
    private static void SetFilterTags(Activity? activity, ProductionOrderListQuery query)
    {
        if (activity is null)
        {
            return;
        }

        activity.SetTag("filter.status_count", query.Statuses.Count);
        activity.SetTag("filter.has_product", query.ProductId is not null);
        activity.SetTag("filter.has_due_range", query.DueFrom is not null || query.DueTo is not null);
        activity.SetTag("filter.has_order_number", query.OrderNumberPattern is not null);
        activity.SetTag("sort", query.Sort.ToApiValue());
        activity.SetTag("dir", query.Direction.ToApiValue());
    }

    private static void Complete(Activity? activity, System.Diagnostics.Metrics.Counter<long> counter, string outcome)
    {
        counter.Add(1, new KeyValuePair<string, object?>("outcome", outcome));
        activity?.SetTag("outcome", outcome);
        if (outcome == Outcomes.Error)
        {
            activity?.SetStatus(ActivityStatusCode.Error);
        }
    }

    [LoggerMessage(EventId = 2001, EventName = "ProductionOrderValidationFailed", Level = LogLevel.Information,
        Message = "Production order {OrderId} failed validation on fields {Fields}")]
    private static partial void LogValidationFailed(ILogger logger, Guid? orderId, string fields);

    [LoggerMessage(EventId = 2002, EventName = "ProductionOrderRuleViolated", Level = LogLevel.Information,
        Message = "Production order {OrderId} rule violated: {Code} ({FromStatus} -> {ToStatus})")]
    private static partial void LogRuleViolated(
        ILogger logger, Guid orderId, string code, ProductionOrderStatus fromStatus, ProductionOrderStatus toStatus);

    [LoggerMessage(EventId = 2003, EventName = "ProductionOrderConcurrencyConflict", Level = LogLevel.Information,
        Message = "Production order {OrderId} save rejected: changed by someone else")]
    private static partial void LogConcurrencyConflict(ILogger logger, Guid orderId);

    [LoggerMessage(EventId = 2004, EventName = "ProductionOrderUnexpectedFailure", Level = LogLevel.Error,
        Message = "Production order {Operation} failed unexpectedly")]
    private static partial void LogUnexpectedFailure(ILogger logger, Exception exception, string operation);

    private sealed class FieldErrors
    {
        private readonly Dictionary<string, List<string>> _errors = new(StringComparer.Ordinal);

        public bool Any => _errors.Count > 0;

        public IEnumerable<string> FieldNames => _errors.Keys;

        public bool Has(string field) => _errors.ContainsKey(field);

        public void Add(string field, string messageId)
        {
            if (!_errors.TryGetValue(field, out var list))
            {
                _errors[field] = list = [];
            }

            list.Add(messageId);
        }

        public IReadOnlyDictionary<string, string[]> ToDictionary() =>
            _errors.ToDictionary(pair => pair.Key, pair => pair.Value.ToArray(), StringComparer.Ordinal);
    }
}
