using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Time.Testing;
using ProductionManagementAI.Application.Health;

namespace ProductionManagementAI.Integration.Tests.Dashboard;

/// <summary>
/// The dashboard's own fixture (DD-003 "Test data isolation"): its own container, a pinned clock, and a database ping
/// a test can make fail or stall. Tests clear production_orders as the owner and insert exactly the rows they need.
/// </summary>
public class DashboardFixture : IntegrationTestFixture
{
    /// <summary>Plant today = 2031-06-11 (12:00 in Tokyo): far from the seed's run date, so seeded rows never matter.</summary>
    public FakeTimeProvider Time { get; } = new(new DateTimeOffset(2031, 6, 11, 3, 0, 0, TimeSpan.Zero));

    /// <summary>When set, replaces the real <c>SELECT 1</c> ping (TC-224).</summary>
    public Func<CancellationToken, Task>? PingOverride { get; set; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(Time);

            var real = services.Single(d => d.ServiceType == typeof(IDatabasePing));
            services.Remove(real);
            services.AddScoped<IDatabasePing>(sp => new SwitchablePing(
                this, (IDatabasePing)ActivatorUtilities.CreateInstance(sp, real.ImplementationType!)));
        });
    }

    public Task ClearOrdersAsync() => ExecuteAsOwnerAsync("DELETE FROM production_orders;");

    private sealed class SwitchablePing(DashboardFixture fixture, IDatabasePing real) : IDatabasePing
    {
        public Task PingAsync(CancellationToken cancellationToken) =>
            fixture.PingOverride is { } fake ? fake(cancellationToken) : real.PingAsync(cancellationToken);
    }
}
