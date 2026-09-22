using ProductionManagementAI.Domain.ProductionOrders;

namespace ProductionManagementAI.Application.Tests.ProductionOrders;

/// <summary>WI-004 REQ-033 (TC-207, unit): the completion time is set on InProgress → Completed and nowhere else.</summary>
public class ProductionOrderCompletionTests
{
    private static readonly DateTimeOffset Created = new(2026, 9, 1, 1, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Started = new(2026, 9, 5, 2, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Finished = new(2026, 9, 10, 3, 30, 0, TimeSpan.Zero);
    private static readonly Guid Product = Guid.Parse("0197e4a0-0000-7000-8000-000000001001");
    private static readonly DateOnly Due = new(2026, 10, 1);

    private static ProductionOrder Draft() => ProductionOrder.Create(Product, 250, Due, null, 2026, 1, Created);

    private static void Save(ProductionOrder order, ProductionOrderStatus status, DateTimeOffset at, string? notes = null) =>
        order.Update(Product, 250, Due, notes, status, at);

    [Fact]
    public void Completing_an_in_progress_order_records_the_save_time()
    {
        var order = Draft();
        Save(order, ProductionOrderStatus.InProgress, Started);

        Save(order, ProductionOrderStatus.Completed, Finished);

        Assert.Equal(Finished, order.CompletedAtUtc);
        Assert.Equal(order.UpdatedAtUtc, order.CompletedAtUtc);
    }

    [Theory]
    [InlineData(ProductionOrderStatus.Draft, ProductionOrderStatus.InProgress)]
    [InlineData(ProductionOrderStatus.Draft, ProductionOrderStatus.Cancelled)]
    [InlineData(ProductionOrderStatus.Draft, ProductionOrderStatus.Draft)]
    [InlineData(ProductionOrderStatus.InProgress, ProductionOrderStatus.Cancelled)]
    [InlineData(ProductionOrderStatus.InProgress, ProductionOrderStatus.InProgress)]
    public void Other_saves_never_record_a_completion_time(ProductionOrderStatus from, ProductionOrderStatus to)
    {
        var order = Draft();
        if (from == ProductionOrderStatus.InProgress)
        {
            Save(order, ProductionOrderStatus.InProgress, Started);
        }

        Save(order, to, Finished);

        Assert.Null(order.CompletedAtUtc);
    }

    [Fact]
    public void Editing_a_completed_order_later_keeps_its_completion_time()
    {
        var order = Draft();
        Save(order, ProductionOrderStatus.InProgress, Started);
        Save(order, ProductionOrderStatus.Completed, Finished);

        Save(order, ProductionOrderStatus.Completed, Finished.AddDays(2), notes: "delivered");

        Assert.Equal(Finished, order.CompletedAtUtc);
        Assert.Equal(Finished.AddDays(2), order.UpdatedAtUtc);
    }

    [Fact]
    public void A_rejected_transition_records_nothing()
    {
        var order = Draft();

        Assert.Throws<DomainRuleViolation>(() => Save(order, ProductionOrderStatus.Completed, Finished));

        Assert.Null(order.CompletedAtUtc);
        Assert.Equal(ProductionOrderStatus.Draft, order.Status);
    }
}
