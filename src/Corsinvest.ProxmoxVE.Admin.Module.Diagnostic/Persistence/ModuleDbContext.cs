using Corsinvest.ProxmoxVE.Admin.Core.Persistence;

namespace Corsinvest.ProxmoxVE.Admin.Module.Diagnostic.Persistence;

public class ModuleDbContext(DbContextOptions<ModuleDbContext> options) : ModuleDbContextBase<ModuleDbContext>(options)
{
    public DbSet<JobResult> JobResults { get; set; } = default!;
    public DbSet<JobDetail> JobDetails { get; set; } = default!;
    public DbSet<IgnoredIssue> IgnoredIssues { get; set; } = default!;

    protected override string SchemaName => "diagnostic";

    protected override void ConfigureEntities(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<JobResult>().HasIndex(a => a.ClusterName);
        modelBuilder.Entity<IgnoredIssue>().HasIndex(a => a.ClusterName);
    }
}
