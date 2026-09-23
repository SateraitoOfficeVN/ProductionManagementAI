using ProductionManagementAI.Application.Dashboard;
using ProductionManagementAI.Domain.ProductionOrders;

namespace ProductionManagementAI.Application.Tests.Dashboard;

/// <summary>003_DD-FN §4: zero-filling, bucket order and the one rounding rule (TC-203, TC-205, TC-211, TC-212, unit).</summary>
public class DashboardMapperTests
{
    private static readonly DashboardWindow Window = DashboardWindow.For(new DateOnly(2026, 9, 23), new TokyoClock(TimeProvider.System));
    private static readonly DateTimeOffset AsOf = new(2026, 9, 23, 5, 0, 0, TimeSpan.Zero);

    private static DashboardRaw Raw(
        StatusCountRow[]? statuses = null,
        WorkloadRow[]? workload = null,
        DashboardOrderRow[]? overdue = null,
        DeliveryRow? delivery = null,
        TrendRow[]? trend = null) =>
        new(
            statuses ?? [],
            workload ?? [],
            overdue ?? [],
            [],
            [],
            delivery ?? new DeliveryRow(0, 0, 0, 0, 0, 0, null),
            trend ?? []);

    [Fact]
    public void Missing_statuses_are_zero_and_total_is_their_sum()
    {
        var response = DashboardMapper.ToResponse(Window, Raw(statuses: [new("Draft", 3), new("Completed", 2)]), AsOf);

        Assert.Equal(new StatusCounts(5, 3, 0, 2, 0), response.StatusCounts);
    }

    [Fact]
    public void Workload_has_exactly_ten_buckets_in_order_with_zeros_filled()
    {
        var response = DashboardMapper.ToResponse(
            Window, Raw(workload: [new(-1, 4, 400), new(2, 1, 10), new(8, 2, 20)]), AsOf);

        var kinds = response.Workload.Select(b => b.Kind).ToArray();
        Assert.Equal(["overdue", "week", "week", "week", "week", "week", "week", "week", "week", "later"], kinds);
        Assert.Equal([4, 0, 0, 1, 0, 0, 0, 0, 0, 2], response.Workload.Select(b => b.OrderCount));
        Assert.Equal(400, response.Workload[0].Quantity);
        Assert.Null(response.Workload[0].WeekStart);
        Assert.Null(response.Workload[^1].WeekStart);
    }

    [Fact]
    public void The_current_week_bucket_starts_today_and_later_weeks_on_Monday()
    {
        var response = DashboardMapper.ToResponse(Window, Raw(), AsOf);

        Assert.Equal(new DateOnly(2026, 9, 23), response.Workload[1].WeekStart);
        Assert.Equal(new DateOnly(2026, 9, 27), response.Workload[1].WeekEnd);
        Assert.Equal(new DateOnly(2026, 9, 28), response.Workload[2].WeekStart);
        Assert.Equal(new DateOnly(2026, 11, 15), response.Workload[8].WeekEnd);
    }

    [Fact]
    public void Trend_has_twelve_weeks_oldest_first_with_zeros_filled()
    {
        var response = DashboardMapper.ToResponse(
            Window, Raw(trend: [new(Window.Week0, 3), new(Window.Week0.AddDays(-28), 5)]), AsOf);

        Assert.Equal(12, response.CompletionTrend.Count);
        Assert.Equal(Window.TrendWeeks, response.CompletionTrend.Select(w => w.WeekStart));
        Assert.Equal([0, 0, 0, 0, 0, 0, 0, 5, 0, 0, 0, 3], response.CompletionTrend.Select(w => w.OrderCount));
        Assert.Equal(Window.Week0.AddDays(6), response.CompletionTrend[^1].WeekEnd);
    }

    [Theory]
    [InlineData(1.78333, 1.8)]
    [InlineData(1.25, 1.3)] // midpoint: away from zero
    [InlineData(13.7, 13.7)]
    public void Lead_time_is_rounded_half_away_from_zero_to_one_decimal(double mean, double expected)
    {
        var response = DashboardMapper.ToResponse(Window, Raw(delivery: new DeliveryRow(0, 0, 0, 0, 3, 2, mean)), AsOf);

        Assert.Equal(expected, response.LeadTime.AverageDays);
        Assert.Equal(3, response.LeadTime.OrderCount);
    }

    [Fact]
    public void Nothing_completed_gives_null_lead_time_and_zero_counts()
    {
        var response = DashboardMapper.ToResponse(Window, Raw(), AsOf);

        Assert.Null(response.LeadTime.AverageDays);
        Assert.Equal(new OnTimeFigure(0, 0, Window.WindowStart), response.OnTime);
        Assert.Equal(0, response.CompletedThisWeek.OrderCount);
        Assert.Equal(Window.Week0, response.CompletedThisWeek.From);
        Assert.Equal(Window.MonthStart, response.CompletedThisMonth.From);
    }

    [Fact]
    public void Group_total_comes_from_the_window_count_and_empty_is_zero()
    {
        var row = new DashboardOrderRow(Guid.NewGuid(), "PO-2026-00001", 5, new DateOnly(2026, 9, 1), "InProgress",
            Guid.NewGuid(), "P-1001", "ブレーキキャリパー", 12);

        var response = DashboardMapper.ToResponse(Window, Raw(overdue: [row]), AsOf);

        Assert.Equal(12, response.Overdue.Total);
        Assert.Equal(ProductionOrderStatus.InProgress, response.Overdue.Orders[0].Status);
        Assert.Equal("P-1001", response.Overdue.Orders[0].Product.Sku);
        Assert.Equal(0, response.DueSoon.Total);
        Assert.Equal(AsOf, response.AsOf);
        Assert.Equal(Window.Today, response.Today);
    }
}
