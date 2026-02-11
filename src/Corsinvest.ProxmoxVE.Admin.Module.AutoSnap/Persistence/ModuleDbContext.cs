using Corsinvest.ProxmoxVE.Admin.Core.Persistence;

namespace Corsinvest.ProxmoxVE.Admin.Module.AutoSnap.Persistence;

public class ModuleDbContext(DbContextOptions<ModuleDbContext> options) : ModuleDbContextBase<ModuleDbContext>(options)
{
    public DbSet<JobSchedule> Jobs { get; set; } = default!;
    public DbSet<JobResult> Results { get; set; } = default!;
    public DbSet<JobWebHook> WebHooks { get; set; } = default!;

    protected override string SchemaName => "autosnap";

    protected override void ConfigureEntities(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<JobSchedule>().HasIndex(a => a.ClusterName);
        modelBuilder.Entity<JobSchedule>().HasIndex(a => a.Label);
        modelBuilder.Entity<JobSchedule>().HasIndex(e => new { e.ClusterName, e.Label }).IsUnique();
        modelBuilder.Entity<JobResult>().HasIndex(a => a.Start);
        modelBuilder.Entity<JobResult>().HasIndex(a => a.End);
    }
}
