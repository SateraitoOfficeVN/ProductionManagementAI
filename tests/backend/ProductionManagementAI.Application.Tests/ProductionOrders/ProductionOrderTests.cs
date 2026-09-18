using ProductionManagementAI.Domain.ProductionOrders;

namespace ProductionManagementAI.Application.Tests.ProductionOrders;

public class ProductionOrderTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 18, 1, 0, 0, TimeSpan.Zero);
    private static readonly Guid ProductA = Guid.Parse("0197e4a0-0000-7000-8000-000000001001");
    private static readonly Guid ProductB = Guid.Parse("0197e4a0-0000-7000-8000-000000001002");
    private static readonly DateOnly Due = new(2026, 10, 1);

    private static ProductionOrder NewDraft() => ProductionOrder.Create(ProductA, 250, Due, "note", 2026, 1, Now);

    private static ProductionOrder InStatus(ProductionOrderStatus status)
    {
        var order = NewDraft();
        if (status == ProductionOrderStatus.Draft)
        {
            return order;
        }

        if (status is ProductionOrderStatus.InProgress or ProductionOrderStatus.Completed)
        {
            order.Update(ProductA, 250, Due, "note", ProductionOrderStatus.InProgress, Now);
        }

        if (status != ProductionOrderStatus.InProgress)
        {
            order.Update(ProductA, 250, Due, "note", status, Now);
        }

        return order;
    }

    [Fact]
    public void Create_StartsAsDraft_WithNormalizedNotesAndTimestamps()
    {
        var order = ProductionOrder.Create(ProductA, 250, Due, "  hello  ", 2026, 42, Now);

        Assert.Equal(ProductionOrderStatus.Draft, order.Status);
        Assert.Equal("hello", order.Notes);
        Assert.Equal((short)2026, order.OrderYear);
        Assert.Equal(42, order.OrderSeq);
        Assert.Equal(Now, order.CreatedAtUtc);
        Assert.Equal(Now, order.UpdatedAtUtc);
        Assert.NotEqual(Guid.Empty, order.Id);
        Assert.True(order.IsProductQuantityEditable);
    }

    [Theory]
    [InlineData(1, null)]
    [InlineData(250, null)]
    [InlineData(999_999_999, null)]
    [InlineData(0, "MSG-E003")]
    [InlineData(-5, "MSG-E003")]
    [InlineData(1_000_000_000, "MSG-E010")]
    public void ValidateQuantity_EnforcesRange(int quantity, string? expected) =>
        Assert.Equal(expected, ProductionOrder.ValidateQuantity(quantity));

    [Fact]
    public void ValidateNotes_Accepts500CodePoints_IncludingEmojiCountedOnce()
    {
        var notes = new string('a', 499) + "😀"; // 500 code points, 501 UTF-16 units

        Assert.Null(ProductionOrder.ValidateNotes(notes));
        Assert.Equal("MSG-E006", ProductionOrder.ValidateNotes(new string('a', 501)));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void NormalizeNotes_EmptyBecomesNull(string? notes) => Assert.Null(ProductionOrder.NormalizeNotes(notes));

    [Theory]
    [InlineData(ProductionOrderStatus.Draft, ProductionOrderStatus.InProgress)]
    [InlineData(ProductionOrderStatus.Draft, ProductionOrderStatus.Cancelled)]
    [InlineData(ProductionOrderStatus.InProgress, ProductionOrderStatus.Completed)]
    [InlineData(ProductionOrderStatus.InProgress, ProductionOrderStatus.Cancelled)]
    [InlineData(ProductionOrderStatus.InProgress, ProductionOrderStatus.InProgress)]
    public void Update_AllowsDefinedTransitions(ProductionOrderStatus from, ProductionOrderStatus to)
    {
        var order = InStatus(from);

        order.Update(ProductA, 250, Due, "note", to, Now.AddHours(1));

        Assert.Equal(to, order.Status);
        Assert.Equal(Now.AddHours(1), order.UpdatedAtUtc);
    }

    [Theory]
    [InlineData(ProductionOrderStatus.Draft, ProductionOrderStatus.Completed)]
    [InlineData(ProductionOrderStatus.InProgress, ProductionOrderStatus.Draft)]
    [InlineData(ProductionOrderStatus.Completed, ProductionOrderStatus.InProgress)]
    [InlineData(ProductionOrderStatus.Completed, ProductionOrderStatus.Cancelled)]
    [InlineData(ProductionOrderStatus.Cancelled, ProductionOrderStatus.Draft)]
    public void Update_RejectsOtherTransitions_WithoutChangingState(ProductionOrderStatus from, ProductionOrderStatus to)
    {
        var order = InStatus(from);

        var ex = Assert.Throws<DomainRuleViolation>(() => order.Update(ProductA, 250, Due, "changed", to, Now.AddHours(1)));

        Assert.Equal("MSG-E007", ex.Code);
        Assert.Equal(from, order.Status);
        Assert.Equal("note", order.Notes);
    }

    [Fact]
    public void Update_AllowsProductAndQuantityChange_WhileDraft()
    {
        var order = NewDraft();

        order.Update(ProductB, 300, Due, null, ProductionOrderStatus.Draft, Now);

        Assert.Equal(ProductB, order.ProductId);
        Assert.Equal(300, order.Quantity);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void Update_RejectsLockedFieldChange_AsAWhole_AfterDraft(bool changeProduct, bool changeQuantity)
    {
        var order = InStatus(ProductionOrderStatus.InProgress);

        var ex = Assert.Throws<DomainRuleViolation>(() => order.Update(
            changeProduct ? ProductB : ProductA,
            changeQuantity ? 999 : 250,
            Due.AddDays(3),
            "other change in the same request",
            ProductionOrderStatus.InProgress,
            Now));

        Assert.Equal("MSG-E008", ex.Code);
        Assert.Equal("note", order.Notes); // DEC-007: nothing from the request applied
        Assert.Equal(Due, order.DueDate);
    }

    [Fact]
    public void Update_LockCheckedBeforeTransition()
    {
        var order = InStatus(ProductionOrderStatus.Completed);

        var ex = Assert.Throws<DomainRuleViolation>(() =>
            order.Update(ProductB, 250, Due, "note", ProductionOrderStatus.Draft, Now));

        Assert.Equal("MSG-E008", ex.Code);
    }

    [Fact]
    public void Update_AllowsDueDateAndNotes_OnTerminalOrder()
    {
        var order = InStatus(ProductionOrderStatus.Completed);

        order.Update(ProductA, 250, Due.AddDays(-30), "delivered", ProductionOrderStatus.Completed, Now);

        Assert.Equal(Due.AddDays(-30), order.DueDate);
        Assert.Equal("delivered", order.Notes);
    }

    [Theory]
    [InlineData(ProductionOrderStatus.Draft, new[] { ProductionOrderStatus.InProgress, ProductionOrderStatus.Cancelled })]
    [InlineData(ProductionOrderStatus.InProgress, new[] { ProductionOrderStatus.Completed, ProductionOrderStatus.Cancelled })]
    [InlineData(ProductionOrderStatus.Completed, new ProductionOrderStatus[0])]
    [InlineData(ProductionOrderStatus.Cancelled, new ProductionOrderStatus[0])]
    public void AllowedNext_MatchesStateMachine(ProductionOrderStatus status, ProductionOrderStatus[] expected) =>
        Assert.Equal(expected, status.AllowedNext());

    [Fact]
    public void Create_WithUnvalidatedValues_Throws() =>
        Assert.Throws<ArgumentException>(() => ProductionOrder.Create(ProductA, 0, Due, null, 2026, 1, Now));
}
