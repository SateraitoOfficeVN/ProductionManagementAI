using Microsoft.AspNetCore.Authorization;
using ProductionManagementAI.Infrastructure;
using ProductionManagementAI.Infrastructure.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration);

// Global fallback: every endpoint requires an authenticated user unless marked [AllowAnonymous]
// (ADR-0002: role checks are enforced server-side, never inferred from a client-supplied claim).
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // Development-only seed data (ADR-0002): placeholder Admin/Operator roles + one seed admin user.
    using var seedScope = app.Services.CreateScope();
    await IdentitySeeder.SeedAsync(seedScope.ServiceProvider, app.Configuration);
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/health", () => Results.Ok()).AllowAnonymous();

app.Run();
