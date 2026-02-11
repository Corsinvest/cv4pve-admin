using Corsinvest.ProxmoxVE.Admin.Core.Persistence;
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth;
using Corsinvest.ProxmoxVE.Admin.Module.System.Settings.Models;

namespace Corsinvest.ProxmoxVE.Admin.Module.System.Persistence;

public class ModuleDbContext(DbContextOptions<ModuleDbContext> options)
    : IdentityDbContext<ApplicationUser,
                        ApplicationRole,
                        string,
                        ApplicationUserClaim,
                        ApplicationUserRole,
                        ApplicationUserLogin,
                        ApplicationRoleClaim,
                        ApplicationUserToken>(options)
{
    public DbSet<UserPermission> UserPermissions { get; set; } = default!;
    public DbSet<RolePermission> RolePermissions { get; set; } = default!;
    public DbSet<SystemSettings> Settings { get; set; } = default!;
    public DbSet<AuditLog> AuditLogs { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("system");
        modelBuilder.ApplyPostgreSqlCaseInsensitiveCollation();

        modelBuilder.Entity<SystemSettings>().HasIndex(a => new { a.Context, a.Section, a.Key });

        modelBuilder.Entity<UserPermission>(entity =>
        {
            entity.HasIndex(a => new { a.UserId, a.PermissionKey, a.Path, a.ClusterName }).IsUnique(false);

            entity.HasOne(a => a.User)
                  .WithMany(a => a.UserPermissions)
                  .HasForeignKey(a => a.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasIndex(a => new { a.RoleId, a.PermissionKey, a.Path, a.ClusterName }).IsUnique(false);

            entity.HasOne(a => a.Role)
                  .WithMany(a => a.RolePermissions)
                  .HasForeignKey(a => a.RoleId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ApplicationUserRole>(entity =>
        {
            entity.HasKey(a => new { a.UserId, a.RoleId });

            entity.HasOne(a => a.User)
                  .WithMany(a => a.UserRoles)
                  .HasForeignKey(a => a.UserId)
                  .IsRequired()
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(a => a.Role)
                  .WithMany(a => a.UserRoles)
                  .HasForeignKey(a => a.RoleId)
                  .IsRequired()
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ApplicationUserClaim>(entity =>
        {
            entity.HasOne(a => a.User)
                  .WithMany(a => a.Claims)
                  .HasForeignKey(a => a.UserId)
                  .IsRequired()
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ApplicationUserLogin>(entity =>
        {
            entity.HasKey(a => new { a.LoginProvider, a.ProviderKey });

            entity.HasOne(a => a.User)
                  .WithMany(a => a.Logins)
                  .HasForeignKey(a => a.UserId)
                  .IsRequired()
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ApplicationUserToken>(entity =>
        {
            entity.HasKey(a => new { a.UserId, a.LoginProvider, a.Name });

            entity.HasOne(a => a.User)
                  .WithMany(a => a.Tokens)
                  .HasForeignKey(a => a.UserId)
                  .IsRequired()
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ApplicationRoleClaim>(entity =>
        {
            entity.HasOne(a => a.Role)
                  .WithMany(a => a.Claims)
                  .HasForeignKey(a => a.RoleId)
                  .IsRequired()
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasIndex(a => new { a.UserName, a.Timestamp });
            entity.HasIndex(a => a.Action);
            entity.HasIndex(a => a.Success);
            entity.HasIndex(a => a.Timestamp);

            entity.HasOne(a => a.User)
                  .WithMany()
                  .HasForeignKey(a => a.UserId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }

    public async Task ExecuteMaintenanceAsync(DatabaseMaintenanceOperation operation, CancellationToken cancellationToken = default)
        => await Database.ExecuteSqlRawAsync(operation switch
        {
            // PostgreSQL specific commands (can be extended for other databases)
            DatabaseMaintenanceOperation.Optimize => "VACUUM ANALYZE",
            DatabaseMaintenanceOperation.Reindex => $"REINDEX DATABASE \"{Database.GetDbConnection().Database}\"",
            _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, "Unknown maintenance operation")
        }, cancellationToken);
}
