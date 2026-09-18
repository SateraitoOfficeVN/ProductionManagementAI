using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProductionManagementAI.Domain.ProductionOrders;
using ProductionManagementAI.Infrastructure.Identity;

namespace ProductionManagementAI.Infrastructure;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<AppUser, AppRole, Guid, IdentityUserClaim<Guid>, IdentityUserRole<Guid>,
        IdentityUserLogin<Guid>, IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>(options)
{
    public DbSet<Product> Products => Set<Product>();

    public DbSet<ProductionOrder> ProductionOrders => Set<ProductionOrder>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Physical table names per docs/en/database/0001-identity-schema.md; EFCore.NamingConventions
        // handles column snake_casing, but table renames from Identity's AspNet* defaults need to be explicit.
        builder.Entity<AppUser>(b =>
        {
            b.ToTable("users");
            b.Property(u => u.DisplayName).HasMaxLength(200);
            b.Property(u => u.Id).HasDefaultValueSql("gen_random_uuid()");
            b.Property(u => u.CreatedAtUtc).HasDefaultValueSql("now()");
            b.HasIndex(u => u.NormalizedUserName).HasDatabaseName("ix_users_normalized_user_name");
            b.HasIndex(u => u.NormalizedEmail).HasDatabaseName("ix_users_normalized_email");
        });

        builder.Entity<AppRole>(b =>
        {
            b.ToTable("roles");
            b.Property(r => r.Id).HasDefaultValueSql("gen_random_uuid()");
            b.HasIndex(r => r.NormalizedName).HasDatabaseName("ix_roles_normalized_name");
        });
        builder.Entity<IdentityUserRole<Guid>>(b => b.ToTable("user_roles"));
        builder.Entity<IdentityUserClaim<Guid>>(b => b.ToTable("user_claims"));
        builder.Entity<IdentityRoleClaim<Guid>>(b => b.ToTable("role_claims"));
        builder.Entity<IdentityUserLogin<Guid>>(b => b.ToTable("user_logins"));
        builder.Entity<IdentityUserToken<Guid>>(b => b.ToTable("user_tokens"));

        // Production-order schema (DB-002): ProductionOrders/ProductionOrderConfigurations.cs.
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
