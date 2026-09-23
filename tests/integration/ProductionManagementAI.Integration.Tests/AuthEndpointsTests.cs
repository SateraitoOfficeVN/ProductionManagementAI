using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ProductionManagementAI.Application.Auth;

namespace ProductionManagementAI.Integration.Tests;

// Covers 0002_ADR's own confirmation criteria and plan.md step 20: migration + seeded roles/user
// (proven implicitly — these tests fail immediately if either is missing), login/me/logout via the
// real HTTP pipeline, and the unauthenticated-401 boundary.
public class AuthEndpointsTests(IntegrationTestFixture fixture) : IClassFixture<IntegrationTestFixture>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task Me_WithoutSession_Returns401()
    {
        using var client = fixture.CreateClient();

        var response = await client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithSeededAdmin_Succeeds_AndMeReturnsUserInfo()
    {
        using var client = fixture.CreateClient();

        var loginResponse = await client.PostAsJsonAsync(
            "/api/auth/login", new LoginRequest("admin", IntegrationTestFixture.SeedAdminPassword), JsonOptions);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var loginBody = await loginResponse.Content.ReadFromJsonAsync<MeResponse>(JsonOptions);
        Assert.NotNull(loginBody);
        Assert.Equal("admin", loginBody!.UserName);
        Assert.Contains("Admin", loginBody.Roles);

        var meResponse = await client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);

        var meBody = await meResponse.Content.ReadFromJsonAsync<MeResponse>(JsonOptions);
        Assert.Equal(loginBody.Id, meBody!.Id);
    }

    [Fact]
    public async Task Login_ThenLogout_Then_MeReturns401()
    {
        using var client = fixture.CreateClient();

        var loginResponse = await client.PostAsJsonAsync(
            "/api/auth/login", new LoginRequest("admin", IntegrationTestFixture.SeedAdminPassword), JsonOptions);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var logoutResponse = await client.PostAsync("/api/auth/logout", content: null);
        Assert.Equal(HttpStatusCode.OK, logoutResponse.StatusCode);

        var meResponse = await client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, meResponse.StatusCode);
    }

    [Fact]
    public async Task Login_WithWrongPassword_Returns401()
    {
        using var client = fixture.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("admin", "wrong-password"), JsonOptions);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
