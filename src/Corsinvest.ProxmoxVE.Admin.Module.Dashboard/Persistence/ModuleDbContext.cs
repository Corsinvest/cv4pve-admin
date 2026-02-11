using Corsinvest.ProxmoxVE.Admin.Core.Persistence;
using ModelDashboard = Corsinvest.ProxmoxVE.Admin.Module.Dashboard.Models.Dashboard;

namespace Corsinvest.ProxmoxVE.Admin.Module.Dashboard.Persistence;

public class ModuleDbContext(DbContextOptions<ModuleDbContext> options) : ModuleDbContextBase<ModuleDbContext>(options)
{
    public DbSet<ModelDashboard> Dashboards { get; set; } = default!;
    public DbSet<Widget> Widgets { get; set; } = default!;

    protected override string SchemaName => "dashboard";

    protected override void ConfigureEntities(ModelBuilder modelBuilder)
        => modelBuilder.Entity<ModelDashboard>()
                       .HasMany(c => c.Widgets)
                       .WithOne(a => a.Dashboard)
                       .OnDelete(DeleteBehavior.Cascade);
}
