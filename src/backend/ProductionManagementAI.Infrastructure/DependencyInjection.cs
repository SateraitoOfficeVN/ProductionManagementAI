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

        // Lockout thresholds mitigate credential-stuffing (ADR-0002 STRIDE: Spoofing);
        // RequireUniqueEmail stays off per DB-001 (no email-based login in scope).
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
            options.Events.OnRedirectToAccessDenied = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            };
        });

        // ADR-0002 (Tampering mitigation): persists Data Protection keys across container restarts
        // so auth cookies aren't silently invalidated. Unset locally outside Docker (ephemeral keys are fine for dev).
        var dataProtectionKeysPath = configuration["DATA_PROTECTION_KEYS_PATH"];
        if (!string.IsNullOrEmpty(dataProtectionKeysPath))
        {
            services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath));
        }

        // Production orders (DD-001-FN). The plant timezone is validated at startup so a bad ID fails fast.
        services.AddOptions<PlantOptions>()
            .BindConfiguration(PlantOptions.SectionName)
            .Validate(o => PlantOptions.IsValidTimeZone(o.TimeZone), "Plant:TimeZone must be a valid IANA timezone ID.")
            .ValidateOnStart();
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IPlantClock, PlantClock>();
        services.AddScoped<IProductionOrderRepository, ProductionOrderRepository>();
        services.AddScoped<IOrderNumberIssuer, OrderNumberIssuer>();

        return services;
    }
}
