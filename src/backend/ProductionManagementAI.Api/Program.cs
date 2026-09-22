using ProductionManagementAI.Application.Dashboard;
using ProductionManagementAI.Application.Health;
using System.Diagnostics;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Npgsql;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using ProductionManagementAI.Api.Controllers;
using ProductionManagementAI.Api.ProductionOrders;
using ProductionManagementAI.Application.ProductionOrders;
using ProductionManagementAI.Infrastructure;
using ProductionManagementAI.Infrastructure.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    // Enums as names only ("InProgress"); an unknown or numeric value is a binding error (DD-001-API).
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false)))
    .ConfigureApiBehaviorOptions(o => o.InvalidModelStateResponseFactory = ProductionOrderProblems.FromModelState);

// RFC 9457 for unhandled errors (ai/rules/backend.md): generic body, never exception detail.
builder.Services.AddProblemDetails(o => o.CustomizeProblemDetails = context =>
{
    var problem = context.ProblemDetails;
    problem.Extensions.Remove("exception");
    problem.Extensions["traceId"] = Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;
    if (problem.Status >= StatusCodes.Status500InternalServerError)
    {
        problem.Type = "urn:pmai:problem:internal";
        problem.Title = "Something went wrong.";
        problem.Detail = null;
        problem.Extensions["code"] = "MSG-E013";
    }
});
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<ProductionOrderService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<SystemHealthService>();

// OpenTelemetry (DD-001 "Observability"). Collected always; exported over OTLP only when
// OTEL_EXPORTER_OTLP_ENDPOINT is set. Npgsql spans carry no parameter values.
var otlpEndpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("ProductionManagementAI.Api"))
    .WithTracing(t =>
    {
        t.AddAspNetCoreInstrumentation().AddNpgsql().AddSource(ProductionOrderTelemetry.Name);
        if (!string.IsNullOrWhiteSpace(otlpEndpoint))
        {
            t.AddOtlpExporter();
        }
    })
    .WithMetrics(m =>
    {
        m.AddAspNetCoreInstrumentation().AddMeter(ProductionOrderTelemetry.Name);
        if (!string.IsNullOrWhiteSpace(otlpEndpoint))
        {
            m.AddOtlpExporter();
        }
    });

// Global fallback: every endpoint requires an authenticated user unless marked [AllowAnonymous]
// (ADR-0002: role checks are enforced server-side, never inferred from a client-supplied claim).
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy(AuthorizationPolicies.ProductionOrderEditor, policy => policy.RequireRole("Admin", "Operator"));
});

var app = builder.Build();

app.UseExceptionHandler();

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

// Exposed for WebApplicationFactory<Program> in the integration test project.
public partial class Program;
