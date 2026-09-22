using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using ProductionManagementAI.Application.ProductionOrders;
using ProductionManagementAI.Domain.ProductionOrders;

namespace ProductionManagementAI.Application.Tests.ProductionOrders;

public class ProductionOrderServiceTests
{
    private static readonly Guid KnownProduct = Guid.Parse("0197e4a0-0000-7000-8000-000000001001");
    private static readonly Guid OtherProduct = Guid.Parse("0197e4a0-0000-7000-8000-000000001002");
    private static readonly DateOnly Today = new(2026, 9, 18);

    private readonly FakeRepository _repository = new([KnownProduct, OtherProduct]);
    private readonly FakeIssuer _issuer = new();
    private readonly ProductionOrderService _service;

    public ProductionOrderServiceTests()
    {
        var time = new FakeTimeProvider(new DateTimeOffset(2026, 9, 18, 1, 0, 0, TimeSpan.Zero));
        _service = new ProductionOrderService(
            _repository, _issuer, new FixedPlantClock(Today), time, NullLogger<ProductionOrderService>.Instance);
    }

    private static CreateProductionOrderRequest ValidCreate(DateOnly? due = null) =>
        new(KnownProduct, 250, due ?? Today.AddDays(10), "note");

    private async Task<ProductionOrderResponse> CreatedOrder()
    {
        var result = await _service.CreateAsync(ValidCreate(), CancellationToken.None);
        return Assert.IsType<Result<ProductionOrderResponse>.Ok>(result).Value;
    }

    private static UpdateProductionOrderRequest UpdateFrom(ProductionOrderResponse o) =>
        new(o.ProductId, o.Quantity, o.DueDate, o.Status, o.Notes, o.Version);

    [Fact]
    public async Task Create_Valid_ReturnsDraftWithIssuedSequenceInPlantYear()
    {
        var order = await CreatedOrder();

        Assert.Equal(ProductionOrderStatus.Draft, order.Status);
        Assert.Equal([ProductionOrderStatus.InProgress, ProductionOrderStatus.Cancelled], order.AllowedNextStatuses);
        Assert.True(order.IsProductQuantityEditable);
        Assert.Equal((short)2026, _issuer.LastYear);
        Assert.True(_repository.Committed);
    }

    [Fact]
    public async Task Create_MissingFields_ReportsAllRequiredErrorsTogether()
    {
        var result = await _service.CreateAsync(new CreateProductionOrderRequest(null, null, null, null), CancellationToken.None);

        var invalid = Assert.IsType<Result<ProductionOrderResponse>.Invalid>(result);
        Assert.Equal(["MSG-E001"], invalid.Errors["productId"]);
        Assert.Equal(["MSG-E003"], invalid.Errors["quantity"]);
        Assert.Equal(["MSG-E004"], invalid.Errors["dueDate"]);
        Assert.Null(_issuer.LastYear); // no number consumed
    }

    [Fact]
    public async Task Create_UnknownProduct_PastDueDate_AndLongNotes_AreFieldErrors()
    {
        var request = new CreateProductionOrderRequest(Guid.NewGuid(), 1_000_000_000, Today.AddDays(-1), new string('x', 501));

        var invalid = Assert.IsType<Result<ProductionOrderResponse>.Invalid>(await _service.CreateAsync(request, CancellationToken.None));

        Assert.Equal(["MSG-E002"], invalid.Errors["productId"]);
        Assert.Equal(["MSG-E010"], invalid.Errors["quantity"]);
        Assert.Equal(["MSG-E005"], invalid.Errors["dueDate"]);
        Assert.Equal(["MSG-E006"], invalid.Errors["notes"]);
    }

    [Fact]
    public async Task Create_DueToday_IsAccepted() =>
        Assert.IsType<Result<ProductionOrderResponse>.Ok>(await _service.CreateAsync(ValidCreate(Today), CancellationToken.None));

    [Fact]
    public async Task Update_UnknownOrder_IsNotFound()
    {
        var request = new UpdateProductionOrderRequest(KnownProduct, 1, Today, ProductionOrderStatus.Draft, null, 0);

        Assert.IsType<Result<ProductionOrderResponse>.NotFound>(
            await _service.UpdateAsync(Guid.NewGuid(), request, CancellationToken.None));
    }

    [Fact]
    public async Task Update_StaleVersion_IsConflict()
    {
        var order = await CreatedOrder();

        var result = await _service.UpdateAsync(order.Id, UpdateFrom(order) with { Version = order.Version + 1 }, CancellationToken.None);

        Assert.IsType<Result<ProductionOrderResponse>.Conflict>(result);
    }

    [Fact]
    public async Task Update_ConcurrentSaveRace_IsConflict()
    {
        var order = await CreatedOrder();
        _repository.FailNextSaveWithConflict = true;

        var result = await _service.UpdateAsync(order.Id, UpdateFrom(order) with { Notes = "x" }, CancellationToken.None);

        Assert.IsType<Result<ProductionOrderResponse>.Conflict>(result);
    }

    [Fact]
    public async Task Update_MissingStatusAndVersion_AreFieldErrors()
    {
        var order = await CreatedOrder();

        var invalid = Assert.IsType<Result<ProductionOrderResponse>.Invalid>(await _service.UpdateAsync(
            order.Id, UpdateFrom(order) with { Status = null, Version = null }, CancellationToken.None));

        Assert.Equal(["MSG-E007"], invalid.Errors["status"]);
        Assert.Equal(["MSG-E009"], invalid.Errors["version"]);
    }

    [Fact]
    public async Task Update_DisallowedTransition_IsRuleViolation()
    {
        var order = await CreatedOrder();

        var result = await _service.UpdateAsync(
            order.Id, UpdateFrom(order) with { Status = ProductionOrderStatus.Completed }, CancellationToken.None);

        Assert.Equal("MSG-E007", Assert.IsType<Result<ProductionOrderResponse>.RuleViolation>(result).Code);
    }

    [Fact]
    public async Task Update_LockedQuantityChange_IsRuleViolation_EvenWithUnknownProduct()
    {
        var order = await CreatedOrder();
        var started = Assert.IsType<Result<ProductionOrderResponse>.Ok>(await _service.UpdateAsync(
            order.Id, UpdateFrom(order) with { Status = ProductionOrderStatus.InProgress }, CancellationToken.None)).Value;

        var result = await _service.UpdateAsync(
            order.Id, UpdateFrom(started) with { ProductId = Guid.NewGuid(), Quantity = 1 }, CancellationToken.None);

        Assert.Equal("MSG-E008", Assert.IsType<Result<ProductionOrderResponse>.RuleViolation>(result).Code);
    }

    [Fact]
    public async Task Update_OverdueOrder_CanBeSavedWithoutChangingDueDate()
    {
        var order = await CreatedOrder();
        _repository.ForceDueDate(order.Id, Today.AddDays(-5)); // became overdue since creation

        var result = await _service.UpdateAsync(
            order.Id, UpdateFrom(order) with { DueDate = Today.AddDays(-5), Status = ProductionOrderStatus.InProgress },
            CancellationToken.None);

        Assert.IsType<Result<ProductionOrderResponse>.Ok>(result);
    }

    [Fact]
    public async Task Update_ChangingDueDateIntoPast_IsFieldError()
    {
        var order = await CreatedOrder();

        var invalid = Assert.IsType<Result<ProductionOrderResponse>.Invalid>(await _service.UpdateAsync(
            order.Id, UpdateFrom(order) with { DueDate = Today.AddDays(-1) }, CancellationToken.None));

        Assert.Equal(["MSG-E005"], invalid.Errors["dueDate"]);
    }

    [Fact]
    public async Task Update_ChangingToUnknownProduct_WhileDraft_IsFieldError()
    {
        var order = await CreatedOrder();

        var invalid = Assert.IsType<Result<ProductionOrderResponse>.Invalid>(await _service.UpdateAsync(
            order.Id, UpdateFrom(order) with { ProductId = Guid.NewGuid() }, CancellationToken.None));

        Assert.Equal(["MSG-E002"], invalid.Errors["productId"]);
    }

    [Fact]
    public async Task Update_ValidTransition_ReturnsNewAllowedStatuses()
    {
        var order = await CreatedOrder();

        var updated = Assert.IsType<Result<ProductionOrderResponse>.Ok>(await _service.UpdateAsync(
            order.Id, UpdateFrom(order) with { Status = ProductionOrderStatus.InProgress, ProductId = OtherProduct },
            CancellationToken.None)).Value;

        Assert.Equal(ProductionOrderStatus.InProgress, updated.Status);
        Assert.Equal(OtherProduct, updated.ProductId);
        Assert.False(updated.IsProductQuantityEditable);
        Assert.Equal([ProductionOrderStatus.Completed, ProductionOrderStatus.Cancelled], updated.AllowedNextStatuses);
    }

    private sealed class FixedPlantClock(DateOnly today) : IPlantClock
    {
        public DateOnly Today => today;

        public short CurrentYear => (short)today.Year;
    }

    private sealed class FakeIssuer : IOrderNumberIssuer
    {
        private int _last;

        public short? LastYear { get; private set; }

        public Task<int> NextAsync(short year, CancellationToken cancellationToken)
        {
            LastYear = year;
            return Task.FromResult(++_last);
        }
    }

    private sealed class FakeRepository(IEnumerable<Guid> productIds) : IProductionOrderRepository
    {
        private readonly HashSet<Guid> _products = [.. productIds];
        private readonly Dictionary<Guid, ProductionOrder> _orders = [];

        public bool Committed { get; private set; }

        public bool FailNextSaveWithConflict { get; set; }

        public Task<IReadOnlyList<ProductResponse>> ListProductsAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<ProductResponse>>([.. _products.Select(id => new ProductResponse(id, "P", "N"))]);

        public Task<bool> ProductExistsAsync(Guid productId, CancellationToken cancellationToken) =>
            Task.FromResult(_products.Contains(productId));

        public Task<ProductionOrder?> FindAsync(Guid id, bool tracked, CancellationToken cancellationToken) =>
            Task.FromResult(_orders.GetValueOrDefault(id));

        public void Add(ProductionOrder order) => _orders[order.Id] = order;

        // The list query itself is exercised against a real database in the integration tests (DD-002-FN §2–§3);
        // here the rows are canned so the service's own logic — the product check, the count-first path and the
        // overdue mapping — can be tested without one.
        public List<ProductionOrderListRow> ListRows { get; } = [];

        public int ListCallCount { get; private set; }

        public Task<int> CountOrdersAsync(ProductionOrderListQuery query, CancellationToken cancellationToken) =>
            Task.FromResult(ListRows.Count);

        public Task<IReadOnlyList<ProductionOrderListRow>> ListOrdersAsync(
            ProductionOrderListQuery query, CancellationToken cancellationToken)
        {
            ListCallCount++;
            return Task.FromResult<IReadOnlyList<ProductionOrderListRow>>(
                [.. ListRows.Skip(query.Skip).Take(query.PageSize)]);
        }

        public void ForceDueDate(Guid id, DateOnly dueDate)
        {
            var o = _orders[id];
            o.Update(o.ProductId, o.Quantity, dueDate, o.Notes, o.Status, o.UpdatedAtUtc);
        }

        public Task<IProductionOrderTransaction> BeginTransactionAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IProductionOrderTransaction>(new Transaction(this));

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            if (FailNextSaveWithConflict)
            {
                FailNextSaveWithConflict = false;
                throw new ConcurrencyConflictException();
            }

            return Task.CompletedTask;
        }

        private sealed class Transaction(FakeRepository owner) : IProductionOrderTransaction
        {
            public Task CommitAsync(CancellationToken cancellationToken)
            {
                owner.Committed = true;
                return Task.CompletedTask;
            }

            public ValueTask DisposeAsync() => ValueTask.CompletedTask;
        }
    }
}
