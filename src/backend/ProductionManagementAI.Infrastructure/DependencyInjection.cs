using ProductionManagementAI.Application.Dashboard;
using ProductionManagementAI.Application.Health;
using ProductionManagementAI.Infrastructure.Dashboard;
using ProductionManagementAI.Infrastructure.Health;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProductionManagementAI.Application.ProductionOrders;
using ProductionManagementAI.Infrastructure.Identity;
using ProductionManagementAI.Infrastructure.ProductionOrders;

namespace ProductionManagementAI.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Resolved lazily from the DI-provided IConfiguration (not the outer `configuration` parameter):
        // that outer reference is read at registration time, before host-building customizations like
        // WebApplicationFactory's ConfigureAppConfiguration overrides (used by integration tests) are
        // guaranteed to have been merged in.
        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            var connectionString = sp.GetRequiredService<IConfiguration>().GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
            options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention();
        });

        // Lockout thresholds mitigate credential-stuffing (0002_ADR STRIDE: Spoofing);
        // RequireUniqueEmail stays off per 000_DB (no email-based login in scope).
        services.AddIdentity<AppUser, AppRole>(options =>
            {
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.AllowedForNewUsers = true;
                options.User.RequireUniqueEmail = false;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        // Identity's cookie middleware defaults to a 302 redirect on auth failure, which breaks a JSON API;
        // rewrite those responses to plain 401/403 so callers get a normal HTTP status instead.
        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Events.OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            };
            // WI-004 DEC-019/DEC-025: the dashboard's 30-second health poll must not keep a session alive. A cookie is
            // renewed from two places — sliding expiration, and Identity's security-stamp revalidation (every 30 min)
            // in OnValidatePrincipal — so both are suppressed for the health path. The stamp is still validated there,
            // so a revoked session is still rejected. Every other request renews as before.
            options.Events.OnCheckSlidingExpiration = context =>
            {
                if (IsHealthPoll(context.HttpContext))
                {
                    context.ShouldRenew = false;
                }

                return Task.CompletedTask;
            };
            var validateSecurityStamp = options.Events.OnValidatePrincipal;
            options.Events.OnValidatePrincipal = async context =>
            {
                await validateSecurityStamp(context);
                if (IsHealthPoll(context.HttpContext))
                {
                    context.ShouldRenew = false;
                }
            };
            options.Events.OnRedirectToAccessDenied = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            };
        });

        // 0002_ADR (Tampering mitigation): persists Data Protection keys across container restarts
        // so auth cookies aren't silently invalidated. Unset locally outside Docker (ephemeral keys are fine for dev).
        var dataProtectionKeysPath = configuration["DATA_PROTECTION_KEYS_PATH"];
        if (!string.IsNullOrEmpty(dataProtectionKeysPath))
        {
            services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath));
        }

        // Production orders (001_DD-FN). The plant timezone is validated at startup so a bad ID fails fast.
        services.AddOptions<PlantOptions>()
            .BindConfiguration(PlantOptions.SectionName)
            .Validate(o => PlantOptions.IsValidTimeZone(o.TimeZone), "Plant:TimeZone must be a valid IANA timezone ID.")
            .ValidateOnStart();
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IPlantClock, PlantClock>();
        services.AddScoped<IProductionOrderRepository, ProductionOrderRepository>();
        services.AddScoped<IOrderNumberIssuer, OrderNumberIssuer>();

        // Dashboard and health (003_DD-FN).
        services.AddScoped<IDashboardReader, DashboardReader>();
        services.AddScoped<IDatabasePing, DatabasePing>();

        return services;
    }

    private static bool IsHealthPoll(HttpContext context) =>
        context.Request.Path.StartsWithSegments(SystemHealthService.HealthPath);
}
