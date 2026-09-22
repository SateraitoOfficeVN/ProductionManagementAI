using System.Net;

namespace ProductionManagementAI.Integration.Tests.Dashboard;

/// <summary>
/// TC-225 (WI-004 DEC-019): the health poll never renews the sign-in session; real use still does. Its own fixture,
/// because it moves the clock forward.
/// </summary>
public class SessionRenewalTests(DashboardFixture fixture) : IClassFixture<DashboardFixture>
{
    [Fact]
    public async Task HealthPoll_DoesNotRenewTheCookie_ButTheDashboardDoes()
    {
        using var client = await fixture.CreateClientAsAsync("Operator");

        // Identity's default cookie lasts 14 days and slides once more than half has elapsed.
        fixture.Time.Advance(TimeSpan.FromDays(8));

        var health = await client.GetAsync("/api/system/health");
        Assert.Equal(HttpStatusCode.OK, health.StatusCode);
        Assert.False(health.Headers.Contains("Set-Cookie"), "the health poll must not reissue the session cookie");

        var dashboard = await client.GetAsync("/api/dashboard");
        Assert.Equal(HttpStatusCode.OK, dashboard.StatusCode);
        Assert.True(dashboard.Headers.Contains("Set-Cookie"), "a real request past half-life renews the session");
    }
}
