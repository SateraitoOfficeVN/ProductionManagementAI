using System.Net;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using ProductionManagementAI.Infrastructure.ProductionOrders;
using static ProductionManagementAI.Integration.Tests.ProductionOrders.ProductionOrderApi;

namespace ProductionManagementAI.Integration.Tests.ProductionOrders;

// Own class = own fixture = fresh database. Since WI-003/WI-004 that database also carries the 124 seeded demo orders
// (002_DB, 003_DB), so the sequence continues from the seeded counter rather than starting at 00001 — which is
// exactly what the seed's counter row exists to guarantee. Everything runs in one test method because the
// assertions depend on order.
public class OrderNumberingTests(IntegrationTestFixture fixture) : IClassFixture<IntegrationTestFixture>
{
    [Fact]
    public async Task Numbers_ContinueFromSeededCounter_AreGapFreeUnderConcurrency_AndOverflowIsRejectedCleanly()
    {
        using var client = await fixture.CreateClientAsAsync("Admin");
        var year = PlantToday.Year;

        // 0. The seed left the counter at the highest seeded sequence, so the next order can't collide with a
        //    seeded order number (002_DB demo seed).
        var seeded = await fixture.ScalarAsOwnerAsync<int>(
            $"SELECT last_seq FROM production_order_number_counters WHERE order_year = {year}");
        Assert.Equal(124, seeded);

        // 1. First order created through the API continues the sequence.
        var first = await client.CreateOrder();
        Assert.Equal($"PO-{year}-{seeded + 1:00000}", first["orderNumber"]!.GetValue<string>());

        // 2. A failed create doesn't consume a number.
        var invalid = await client.PostJson("/api/production-orders", ValidCreate(product: Guid.NewGuid()));
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        Assert.Equal($"PO-{year}-{seeded + 2:00000}", (await client.CreateOrder())["orderNumber"]!.GetValue<string>());

        // 3. 20 concurrent creates get 20 distinct, contiguous numbers.
        var results = await Task.WhenAll(Enumerable.Range(0, 20).Select(_ => client.CreateOrder()));
        var sequences = results.Select(r => int.Parse(r["orderNumber"]!.GetValue<string>()[^5..])).Order().ToArray();
        Assert.Equal(Enumerable.Range(seeded + 3, 20), sequences);

        // 4. Overflow: the 100,000th order of a year fails with a generic 500, rolls back, and inserts nothing.
        await fixture.ExecuteAsOwnerAsync($"UPDATE production_order_number_counters SET last_seq = 99999 WHERE order_year = {year}");
        var before = await fixture.ScalarAsOwnerAsync<long>("SELECT count(*) FROM production_orders");

        var overflow = await client.PostJson("/api/production-orders", ValidCreate());

        Assert.Equal(HttpStatusCode.InternalServerError, overflow.StatusCode);
        Assert.Equal("application/problem+json", overflow.Content.Headers.ContentType!.MediaType);
        var problem = await overflow.Body();
        Assert.Equal("MSG-E013", problem["code"]!.GetValue<string>());
        Assert.Null(problem["detail"]);
        Assert.Null(problem["exception"]);
        Assert.DoesNotContain("Postgres", await overflow.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);
        Assert.Equal(before, await fixture.ScalarAsOwnerAsync<long>("SELECT count(*) FROM production_orders"));
        Assert.Equal(99999, await fixture.ScalarAsOwnerAsync<int>(
            $"SELECT last_seq FROM production_order_number_counters WHERE order_year = {year}"));
    }

    [Fact]
    public void PlantClock_YearBoundary_UsesTokyoTime()
    {
        // 2026-12-31T15:30Z is 2027-01-01 00:30 in Asia/Tokyo (DEC-017).
        var time = new FakeTimeProvider(new DateTimeOffset(2026, 12, 31, 15, 30, 0, TimeSpan.Zero));
        var clock = new PlantClock(time, Options.Create(new PlantOptions { TimeZone = "Asia/Tokyo" }));

        Assert.Equal(new DateOnly(2027, 1, 1), clock.Today);
        Assert.Equal((short)2027, clock.CurrentYear);

        // 14:30Z is still 23:30 JST on 2026-12-31. FakeTimeProvider can't move backwards, so use a second clock.
        var justBefore = new PlantClock(
            new FakeTimeProvider(new DateTimeOffset(2026, 12, 31, 14, 30, 0, TimeSpan.Zero)),
            Options.Create(new PlantOptions { TimeZone = "Asia/Tokyo" }));
        Assert.Equal((short)2026, justBefore.CurrentYear);
    }

    [Theory]
    [InlineData("Asia/Tokyo", true)]
    [InlineData("Not/AZone", false)]
    [InlineData("", false)]
    public void PlantOptions_ValidatesTimeZone(string id, bool valid) => Assert.Equal(valid, PlantOptions.IsValidTimeZone(id));
}
