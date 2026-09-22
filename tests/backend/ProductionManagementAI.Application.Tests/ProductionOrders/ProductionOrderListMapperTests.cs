using ProductionManagementAI.Application.ProductionOrders;
using ProductionManagementAI.Domain.ProductionOrders;

namespace ProductionManagementAI.Application.Tests.ProductionOrders;

// DD-002-FN §6 / BD-002 M-08, unit level (U): the overdue rule and the row → list item mapping.
public class ProductionOrderListMapperTests
{
    private static readonly DateOnly PlantToday = new(2026, 9, 22);

    private static ProductionOrderListRow Row(DateOnly dueDate, ProductionOrderStatus status) => new(
        Guid.NewGuid(),
        "PO-2026-00042",
        Guid.NewGuid(),
        "P-1004",
        "Drive shaft",
        250,
        dueDate,
        status,
        new DateTimeOffset(2026, 9, 20, 1, 12, 3, TimeSpan.Zero));

    [Theory]
    [InlineData(ProductionOrderStatus.Draft)]
    [InlineData(ProductionOrderStatus.InProgress)]
    public void PastDue_ActiveOrder_IsOverdue(ProductionOrderStatus status) =>
        Assert.True(ProductionOrderListMapper.ToListItem(Row(PlantToday.AddDays(-1), status), PlantToday).IsOverdue);

    [Theory]
    [InlineData(ProductionOrderStatus.Completed)]
    [InlineData(ProductionOrderStatus.Cancelled)]
    public void PastDue_TerminalOrder_IsNotOverdue(ProductionOrderStatus status) =>
        Assert.False(ProductionOrderListMapper.ToListItem(Row(PlantToday.AddDays(-30), status), PlantToday).IsOverdue);

    [Fact]
    public void DueToday_IsNotOverdue() =>
        Assert.False(ProductionOrderListMapper.ToListItem(
            Row(PlantToday, ProductionOrderStatus.Draft), PlantToday).IsOverdue);

    [Fact]
    public void DueTomorrow_IsNotOverdue() =>
        Assert.False(ProductionOrderListMapper.ToListItem(
            Row(PlantToday.AddDays(1), ProductionOrderStatus.InProgress), PlantToday).IsOverdue);

    [Fact]
    public void Mapping_CarriesTheRowsDisplayedFields()
    {
        var row = Row(PlantToday.AddDays(8), ProductionOrderStatus.InProgress);

        var item = ProductionOrderListMapper.ToListItem(row, PlantToday);

        Assert.Equal(row.Id, item.Id);
        Assert.Equal(row.OrderNumber, item.OrderNumber);
        Assert.Equal(new ProductSummary(row.ProductId, "P-1004", "Drive shaft"), item.Product);
        Assert.Equal(250, item.Quantity);
        Assert.Equal(row.DueDate, item.DueDate);
        Assert.Equal(ProductionOrderStatus.InProgress, item.Status);
        Assert.Equal(row.UpdatedAt, item.UpdatedAt);
    }

    [Theory]
    [InlineData(ProductionOrderSort.DueDate, "dueDate")]
    [InlineData(ProductionOrderSort.OrderNumber, "orderNumber")]
    [InlineData(ProductionOrderSort.Product, "product")]
    [InlineData(ProductionOrderSort.Quantity, "quantity")]
    [InlineData(ProductionOrderSort.Status, "status")]
    [InlineData(ProductionOrderSort.UpdatedAt, "updatedAt")]
    public void SortKeys_EchoTheirApiSpelling(ProductionOrderSort sort, string expected) =>
        Assert.Equal(expected, sort.ToApiValue());

    [Theory]
    [InlineData(SortDirection.Asc, "asc")]
    [InlineData(SortDirection.Desc, "desc")]
    public void Directions_EchoTheirApiSpelling(SortDirection direction, string expected) =>
        Assert.Equal(expected, direction.ToApiValue());
}
