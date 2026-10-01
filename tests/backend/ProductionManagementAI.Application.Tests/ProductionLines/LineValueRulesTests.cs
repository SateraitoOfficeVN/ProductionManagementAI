using ProductionManagementAI.Application.ProductionLines;
using ProductionManagementAI.Domain.ProductionOrders;

namespace ProductionManagementAI.Application.Tests.ProductionLines;

/// <summary>Verifies exact decimal/generation boundaries and the Domain assignment lock.</summary>
public sealed class LineValueRulesTests
{
    /// <summary>Verifies TC-328 preserves accepted decimal strings exactly.</summary>
    [Theory]
    [InlineData("0.001")]
    [InlineData("1")]
    [InlineData("1.230")]
    [InlineData("24")]
    public void ValidHoursUseExactDecimals(string text)
    {
        Assert.True(LineValueRules.TryDecimal(text, 24, out var value));
        Assert.True(value > 0 && value <= 24);
    }

    /// <summary>Verifies TC-328 rejects lexical and range violations without rounding.</summary>
    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("24.001")]
    [InlineData("1.2340")]
    [InlineData("1.")]
    [InlineData(".1")]
    [InlineData("01")]
    [InlineData("1e0")]
    [InlineData("+1")]
    [InlineData("1,000")]
    [InlineData("1 ")]
    [InlineData("1\n")]
    [InlineData("NaN")]
    [InlineData("Infinity")]
    [InlineData("１")]
    public void InvalidHoursAreRejected(string text) => Assert.False(LineValueRules.TryDecimal(text, 24, out _));

    /// <summary>Verifies TC-328/337 the coefficient boundary and canonical output.</summary>
    [Fact]
    public void MinutesBoundaryAndFormattingAreExact()
    {
        Assert.True(LineValueRules.TryDecimal("999999999.999", 999999999.999m, out var max));
        Assert.Equal(999999999.999m, max);
        Assert.False(LineValueRules.TryDecimal("1000000000", 999999999.999m, out _));
        Assert.Equal("1.23", LineValueRules.Format(1.230m));
        Assert.Equal("0.001", LineValueRules.Format(.001m));
    }

    /// <summary>Verifies TC-337 revisions beyond JavaScript precision remain exact opaque tokens.</summary>
    [Fact]
    public void RevisionAndVersionBoundsAreCanonical()
    {
        Assert.True(LineValueRules.TryRevision("9223372036854775807", out var revision));
        Assert.Equal(long.MaxValue, revision);
        Assert.False(LineValueRules.TryRevision("9223372036854775808", out _));
        Assert.False(LineValueRules.TryRevision("01", out _));
        Assert.False(LineValueRules.TryRevision("1\n", out _));
        Assert.True(LineValueRules.TryVersion("4294967295", out var version));
        Assert.Equal(uint.MaxValue, version);
        Assert.False(LineValueRules.TryVersion("4294967296", out _));
    }

    /// <summary>Verifies TC-327 counts Unicode scalars after trimming.</summary>
    [Fact]
    public void TextBoundsCountCodePoints()
    {
        Assert.True(LineValueRules.TryText("  " + string.Concat(Enumerable.Repeat("😀", 50)) + "  ", 50, out var code));
        Assert.Equal(100, code.Length);
        Assert.False(LineValueRules.TryText(code + "x", 50, out _));
        Assert.False(LineValueRules.TryText("　 	", 50, out _));
    }

    /// <summary>Verifies TC-331 keeps non-Draft assignments and legacy null immutable.</summary>
    [Theory]
    [InlineData(ProductionOrderStatus.InProgress)]
    [InlineData(ProductionOrderStatus.Cancelled)]
    [InlineData(ProductionOrderStatus.Completed)]
    public void NonDraftAssignmentCannotChange(ProductionOrderStatus status)
    {
        var now = DateTimeOffset.UtcNow;
        var order = ProductionOrder.Create(Guid.NewGuid(), 1, DateOnly.FromDateTime(now.Date), null, 2026, 1, now);
        if (status == ProductionOrderStatus.Completed)
            order.Update(order.ProductId, 1, order.DueDate, null, ProductionOrderStatus.InProgress, now);
        order.Update(order.ProductId, 1, order.DueDate, null, status, now);
        order.AssignLine(null);
        Assert.Equal("LINE_LOCKED", Assert.Throws<DomainRuleViolation>(() => order.AssignLine(Guid.NewGuid())).Code);
        Assert.Null(order.LineId);
    }
}
