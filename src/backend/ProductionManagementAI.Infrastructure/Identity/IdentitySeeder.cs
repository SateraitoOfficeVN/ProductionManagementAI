using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ProductionManagementAI.Infrastructure.Identity;

/// <summary>
/// Development-only seed data (0002_ADR): placeholder Admin/Operator roles and one seed admin user.
/// Never call this outside Development — the password comes from an env var with no hardcoded fallback,
/// and callers must not log or persist it anywhere (including evidence.md).
/// </summary>
public static class IdentitySeeder
{
    public const string AdminRoleName = "Admin";
    public const string OperatorRoleName = "Operator";
    private const string SeedAdminUserName = "admin";

    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        // Checked before any DB access: fail fast on missing configuration, not on a DB error that would mask it.
        var seedAdminPassword = configuration["SEED_ADMIN_PASSWORD"];
        if (string.IsNullOrEmpty(seedAdminPassword))
        {
            throw new InvalidOperationException(
                "SEED_ADMIN_PASSWORD must be set in Development to seed the admin user; no hardcoded fallback is provided.");
        }

        var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
        var userManager = services.GetRequiredService<UserManager<AppUser>>();

        foreach (var roleName in new[] { AdminRoleName, OperatorRoleName })
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var result = await roleManager.CreateAsync(new AppRole { Name = roleName });
                ThrowIfFailed(result, $"seed role '{roleName}'");
            }
        }

        var adminUser = await userManager.FindByNameAsync(SeedAdminUserName);
        if (adminUser is null)
        {
            adminUser = new AppUser { UserName = SeedAdminUserName, DisplayName = "Seed Admin" };
            var result = await userManager.CreateAsync(adminUser, seedAdminPassword);
            ThrowIfFailed(result, "seed admin user");
        }

        if (!await userManager.IsInRoleAsync(adminUser, AdminRoleName))
        {
            var result = await userManager.AddToRoleAsync(adminUser, AdminRoleName);
            ThrowIfFailed(result, "assign Admin role to seed admin user");
        }
    }

    private static void ThrowIfFailed(IdentityResult result, string action)
    {
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to {action}: {errors}");
        }
    }
}
