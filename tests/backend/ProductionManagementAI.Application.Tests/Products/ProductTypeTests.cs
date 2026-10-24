using ProductionManagementAI.Application.ProductionOrders;
using ProductionManagementAI.Application.Products;
using ProductionManagementAI.Domain.ProductionOrders;

namespace ProductionManagementAI.Application.Tests.Products;

/// <summary>WI-017 product type codes and list filter parsing (004_DD-API-TYPE).</summary>
public sealed class ProductTypeTests
{
    [Theory]
    [InlineData("FINISHED", ProductType.Finished)]
    [InlineData("SEMI_FINISHED", ProductType.SemiFinished)]
    [InlineData("PART", ProductType.Part)]
    [InlineData("RAW_MATERIAL", ProductType.RawMaterial)]
    public void CodesRoundTrip(string code, ProductType type)
    {
        Assert.True(ProductTypeCodes.TryParse(code, out var parsed));
        Assert.Equal(type, parsed);
        Assert.Equal(code, ProductTypeCodes.ToCode(type));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("part")]
    [InlineData(" PART")]
    [InlineData("Part")]
    [InlineData("完成品")]
    [InlineData("0")]
    public void UnknownCodesAreRejected(string? code) => Assert.False(ProductTypeCodes.TryParse(code, out _));

    [Fact]
    public void CodesAreListedInDisplayOrder() =>
        Assert.Equal(["FINISHED", "SEMI_FINISHED", "PART", "RAW_MATERIAL"], ProductTypeCodes.All);

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void AbsentListTypeMeansAllTypes(string? type)
    {
        var result = ProductMasterListQuery.TryCreate(new ProductMasterListRequest(null, null, null, Type: type));
        Assert.Null(Assert.IsType<Result<ProductMasterListQuery>.Ok>(result).Value.Type);
    }

    [Fact]
    public void KnownListTypeIsApplied()
    {
        var result = ProductMasterListQuery.TryCreate(new ProductMasterListRequest(null, null, null, Type: "PART"));
        Assert.Equal(ProductType.Part, Assert.IsType<Result<ProductMasterListQuery>.Ok>(result).Value.Type);
    }

    [Fact]
    public void UnknownListTypeIsReportedWithOtherQueryErrors()
    {
        var result = ProductMasterListQuery.TryCreate(new ProductMasterListRequest(null, null, "0", Type: "X"));
        var errors = Assert.IsType<Result<ProductMasterListQuery>.Invalid>(result).Errors;
        Assert.Contains("type", errors.Keys);
        Assert.Contains("page", errors.Keys);
    }
}
