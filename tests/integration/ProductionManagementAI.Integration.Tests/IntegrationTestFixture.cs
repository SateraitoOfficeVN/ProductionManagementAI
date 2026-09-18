using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using ProductionManagementAI.Application.Auth;
using ProductionManagementAI.Infrastructure;
using ProductionManagementAI.Infrastructure.Identity;
using Testcontainers.PostgreSql;

namespace ProductionManagementAI.Integration.Tests;

public class IntegrationTestFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string SeedAdminPassword = "Test-Admin-Password-1!";
    public const string TestUserPassword = "Test-User-Password-1!";

    // Test-only credential for the restricted runtime login (DEC-016); set on a throwaway container.
    private const string AppDbPassword = "Test-App-Db-Password-1!";

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17").Build();

    /// <summary>Owner connection: migrations and test-only setup SQL.</summary>
    public string OwnerConnectionString => _postgres.GetConnectionString();

    /// <summary>What the app uses at runtime, exactly as in Compose: the restricted pmai_app login.</summary>
    public string AppConnectionString => new NpgsqlConnectionStringBuilder(OwnerConnectionString)
    {
        Username = "pmai_app",
        Password = AppDbPassword,
    }.ToString();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        // Migrate as the owner before the factory builds its host: IdentitySeeder (run during host startup,
        // see Program.cs) queries roles/users immediately and needs the schema to already exist. The
        // AddProductionOrders migration creates pmai_app (NOLOGIN) and its grants; the password is set here,
        // mirroring deploy/db/init/10-app-login.sh.
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(OwnerConnectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

        await using var db = new AppDbContext(options);
        await db.Database.MigrateAsync();
        await ExecuteAsOwnerAsync($"ALTER ROLE pmai_app LOGIN PASSWORD '{AppDbPassword}'");
    }

    public async Task ExecuteAsOwnerAsync(string sql)
    {
        await using var connection = new NpgsqlConnection(OwnerConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }

    public async Task<T?> ScalarAsOwnerAsync<T>(string sql)
    {
        await using var connection = new NpgsqlConnection(OwnerConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        var value = await command.ExecuteScalarAsync();
        return value is null or DBNull ? default : (T)value;
    }

    /// <summary>A signed-in client for a fresh user with the given role, or no role at all when <paramref name="role"/> is null.</summary>
    public async Task<HttpClient> CreateClientAsAsync(string? role)
    {
        var userName = $"user-{Guid.NewGuid():N}"[..20];
        using (var scope = Services.CreateScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var user = new AppUser { UserName = userName, DisplayName = userName };
            var created = await users.CreateAsync(user, TestUserPassword);
            if (!created.Succeeded)
            {
                throw new InvalidOperationException(string.Join("; ", created.Errors.Select(e => e.Description)));
            }

            if (role is not null)
            {
                await users.AddToRoleAsync(user, role);
            }
        }

        var client = CreateClient();
        var login = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(userName, TestUserPassword));
        if (login.StatusCode != HttpStatusCode.OK)
        {
            throw new InvalidOperationException($"Test login failed: {login.StatusCode}");
        }

        return client;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(Environments.Development);
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = AppConnectionString,
                ["SEED_ADMIN_PASSWORD"] = SeedAdminPassword,
            });
        });
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}
