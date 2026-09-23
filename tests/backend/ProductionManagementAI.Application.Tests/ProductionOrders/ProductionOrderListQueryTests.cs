using ProductionManagementAI.Application.ProductionOrders;
using ProductionManagementAI.Domain.ProductionOrders;

namespace ProductionManagementAI.Application.Tests.ProductionOrders;

// 002_DD module 5, unit level (U): the list query's validation and normalization (002_BD V-09–V-13).
public class ProductionOrderListQueryTests
{
    private static ProductionOrderListRequest Request(
        string[]? status = null,
        string? productId = null,
        string? dueFrom = null,
        string? dueTo = null,
        string? orderNumber = null,
        string? sort = null,
        string? dir = null,
        string? page = null,
        string? pageSize = null) =>
        new(status, productId, dueFrom, dueTo, orderNumber, sort, dir, page, pageSize);

    private static ProductionOrderListQuery Valid(ProductionOrderListRequest request) =>
        Assert.IsType<Result<ProductionOrderListQuery>.Ok>(ProductionOrderListQuery.TryCreate(request)).Value;

    private static IReadOnlyDictionary<string, string[]> Errors(ProductionOrderListRequest request) =>
        Assert.IsType<Result<ProductionOrderListQuery>.Invalid>(ProductionOrderListQuery.TryCreate(request)).Errors;

    [Fact]
    public void EmptyRequest_UsesDefaults_AndHasNoFilter()
    {
        var query = Valid(Request());

        Assert.Empty(query.Statuses);
        Assert.Null(query.ProductId);
        Assert.Null(query.OrderNumberPattern);
        Assert.Equal(ProductionOrderSort.DueDate, query.Sort);
        Assert.Equal(SortDirection.Asc, query.Direction);
        Assert.Equal(1, query.Page);
        Assert.Equal(20, query.PageSize);
        Assert.False(query.HasFilter);
        Assert.Equal(0, query.Skip);
    }

    [Fact]
    public void Statuses_AreParsedAndDeduplicated()
    {
        var query = Valid(Request(status: ["Draft", "InProgress", "Draft"]));

        Assert.Equal([ProductionOrderStatus.Draft, ProductionOrderStatus.InProgress], query.Statuses);
        Assert.True(query.HasFilter);
    }

    [Theory]
    [InlineData("Shipped")]
    [InlineData("3")]
    [InlineData("draft;drop")]
    public void UnknownStatus_IsRejected(string value) =>
        Assert.Equal(["MSG-E018"], Errors(Request(status: [value]))["status"]);

    [Fact]
    public void UnparseableProductId_IsRejectedAsUnknownProduct() =>
        Assert.Equal(["MSG-E002"], Errors(Request(productId: "not-a-guid"))["productId"]);

    [Fact]
    public void DueRange_IsParsed_AndInclusiveBoundsAreKept()
    {
        var query = Valid(Request(dueFrom: "2026-09-01", dueTo: "2026-09-30"));

        Assert.Equal(new DateOnly(2026, 9, 1), query.DueFrom);
        Assert.Equal(new DateOnly(2026, 9, 30), query.DueTo);
    }

    [Theory]
    [InlineData("2026-13-01")]
    [InlineData("01/09/2026")]
    [InlineData("2026-09-31")]
    public void MalformedDate_IsRejected(string value) =>
        Assert.Equal(["MSG-E016"], Errors(Request(dueFrom: value))["dueFrom"]);

    [Fact]
    public void InvertedDueRange_IsRejected() =>
        Assert.Equal(["MSG-E017"], Errors(Request(dueFrom: "2026-10-01", dueTo: "2026-09-01"))["dueFrom"]);

    [Fact]
    public void EqualDueRange_IsAccepted_SoASingleDayCanBeSelected()
    {
        var query = Valid(Request(dueFrom: "2026-09-15", dueTo: "2026-09-15"));

        Assert.Equal(query.DueFrom, query.DueTo);
    }

    [Fact]
    public void OrderNumberFragment_IsTrimmedUpperCasedAndWrapped() =>
        Assert.Equal("%PO-2026-00042%", Valid(Request(orderNumber: "  po-2026-00042 ")).OrderNumberPattern);

    [Fact]
    public void BlankOrderNumberFragment_CountsAsAbsent()
    {
        var query = Valid(Request(orderNumber: "   "));

        Assert.Null(query.OrderNumberPattern);
        Assert.False(query.HasFilter);
    }

    [Theory]
    [InlineData("100%", "%100\\%%")]
    [InlineData("A_B", "%A\\_B%")]
    [InlineData("x\\y", "%X\\\\Y%")]
    public void WildcardCharactersInTheFragment_AreEscapedSoASearchCannotWidenItself(string input, string expected) =>
        Assert.Equal(expected, Valid(Request(orderNumber: input)).OrderNumberPattern);

    [Fact]
    public void OrderNumberFragment_LongerThan20Characters_IsRejected() =>
        Assert.Equal(["MSG-E015"], Errors(Request(orderNumber: new string('X', 21)))["orderNumber"]);

    [Fact]
    public void OrderNumberFragment_OfExactly20Characters_IsAccepted() =>
        Assert.NotNull(Valid(Request(orderNumber: new string('X', 20))).OrderNumberPattern);

    [Theory]
    [InlineData("orderNumber", ProductionOrderSort.OrderNumber)]
    [InlineData("product", ProductionOrderSort.Product)]
    [InlineData("quantity", ProductionOrderSort.Quantity)]
    [InlineData("dueDate", ProductionOrderSort.DueDate)]
    [InlineData("status", ProductionOrderSort.Status)]
    [InlineData("updatedAt", ProductionOrderSort.UpdatedAt)]
    public void EverySortKeyInTheAllowList_IsAccepted(string value, ProductionOrderSort expected) =>
        Assert.Equal(expected, Valid(Request(sort: value)).Sort);

    [Theory]
    [InlineData("notes")]
    [InlineData("quantity; DROP TABLE production_orders")]
    [InlineData("1")]
    public void SortKeyOutsideTheAllowList_IsRejected_NotDefaulted(string value) =>
        Assert.Equal(["MSG-E019"], Errors(Request(sort: value))["sort"]);

    [Fact]
    public void Direction_IsAllowListed() =>
        Assert.Equal(["MSG-E019"], Errors(Request(dir: "sideways"))["dir"]);

    [Theory]
    [InlineData("10")]
    [InlineData("20")]
    [InlineData("50")]
    [InlineData("100")]
    public void AllowedPageSizes_AreAccepted(string value) =>
        Assert.Equal(int.Parse(value), Valid(Request(pageSize: value)).PageSize);

    [Theory]
    [InlineData("0")]
    [InlineData("25")]
    [InlineData("101")]
    [InlineData("1000000")]
    [InlineData("abc")]
    [InlineData("-20")]
    public void PageSizeOutsideTheAllowList_IsRejected_SoNoRequestCanPullAnUnboundedResult(string value) =>
        Assert.Equal(["MSG-E019"], Errors(Request(pageSize: value))["pageSize"]);

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("abc")]
    [InlineData("100001")]
    public void PageOutsideItsBounds_IsRejected(string value) =>
        Assert.Equal(["MSG-E019"], Errors(Request(page: value))["page"]);

    [Fact]
    public void PageBeyondTheLastPage_IsStillAValidQuery()
    {
        // The API cannot know the last page without querying; an empty page is the designed answer (REQ-024).
        var query = Valid(Request(page: "9999"));

        Assert.Equal(9999, query.Page);
        Assert.Equal(9998 * 20, query.Skip);
    }

    [Fact]
    public void EveryOffendingParameter_IsReportedTogether()
    {
        var errors = Errors(Request(
            status: ["Nope"], dueFrom: "oops", orderNumber: new string('X', 21), sort: "notes", pageSize: "7"));

        Assert.Equal(
            ["dueFrom", "orderNumber", "pageSize", "sort", "status"],
            errors.Keys.Order().ToArray());
    }

    [Fact]
    public void Skip_FollowsPageAndPageSize() =>
        Assert.Equal(100, Valid(Request(page: "3", pageSize: "50")).Skip);
}
