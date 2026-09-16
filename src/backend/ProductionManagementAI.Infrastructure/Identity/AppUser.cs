using Microsoft.AspNetCore.Identity;

namespace ProductionManagementAI.Infrastructure.Identity;

public class AppUser : IdentityUser<Guid>
{
    public required string DisplayName { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}
