using ProductionManagementAI.Application.ProductionLines;

namespace ProductionManagementAI.Application.Tests.ProductionLines;

/// <summary>Verifies normalized commands and submitted paths before any persistence call.</summary>
public sealed class LineValidationTests
{
    [Theory]
    [InlineData("10001")]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("1.0")]
    [InlineData("１")]
    [InlineData(" 1")]
    [InlineData("")]
    public void RejectsNonAsciiOrOutOfRangePages(string page) => Assert.Equal(400, LineValidation.Query(null, page).Problem?.Status);

    [Fact]
    public void DefaultListIsActiveAndSearchUsesUnicodeCodePoints()
    {
        var parsed = LineValidation.Query("  ｋｇ%_  ", null);
        Assert.Equal(new LineQuery("ｋｇ%_", 1), parsed.Value);
        Assert.Null(LineValidation.Query(string.Concat(Enumerable.Repeat("😀", 100)), "10000").Problem);
        Assert.Equal("q", Assert.Single(LineValidation.Query(string.Concat(Enumerable.Repeat("😀", 101)), "1").Problem?.Errors
            ?? throw new InvalidOperationException()).Key);
    }
    [Fact]
    public void EmptyArraysAreValidButMissingAndOversizeArraysAreNot()
    {
        Assert.Null(LineValidation.Create(new(" L ", " 名前 ", "8.000", [])).Problem);
        Assert.NotNull(LineValidation.Create(new("L", "Name", "8", null)).Problem);
        var rows = Enumerable.Range(0, 1001).Select(_ => new LineProductInput("add", Guid.NewGuid(), "1", "kg", "0", true)).ToArray();
        Assert.Contains("products", LineValidation.Create(new("L", "Name", "8", rows)).Problem?.Errors?.Keys ?? []);
    }
    [Fact]
    public void DuplicateIdentitiesAndInvalidFieldsRetainSubmittedRowPaths()
    {
        var id = Guid.NewGuid();
        var parsed = LineValidation.Update(new("Name", "8", "4294967295", [
            new("add", id, "1", "kg", "0", true), new("add", id, "1.2340", "kg", "00", false)]));
        var errors = parsed.Problem?.Errors ?? throw new InvalidOperationException();
        Assert.Contains("productChanges[1].productId", errors.Keys);
        Assert.Contains("productChanges[1].minutesPerUnit", errors.Keys);
        Assert.Contains("productChanges[1].expectedUnitRevision", errors.Keys);
        Assert.Contains("productChanges[1].confirmUnit", errors.Keys);
    }
    [Fact]
    public void RetirementCannotCarryTimingAndSetTimingRequiresExplicitBoolean()
    {
        Assert.NotNull(LineValidation.Update(new("Name", "8", "1", [new("retire", Guid.NewGuid(), "1")])).Problem);
        Assert.NotNull(LineValidation.Update(new("Name", "8", "1", [new("setTiming", Guid.NewGuid(), "1", "kg", "0")])).Problem);
        Assert.Null(LineValidation.Update(new("Name", "8", "1", [new("setTiming", Guid.NewGuid(), "1", "kg", "0", false)])).Problem);
    }
}
