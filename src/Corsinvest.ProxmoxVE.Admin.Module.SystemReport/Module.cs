using Corsinvest.ProxmoxVE.Admin.Core.Helpers;
using Corsinvest.ProxmoxVE.Admin.Core.Modularity;
using Corsinvest.ProxmoxVE.Admin.Core.Persistence;
using Corsinvest.ProxmoxVE.Admin.Module.SystemReport.Persistence;
using Microsoft.Extensions.Configuration;

namespace Corsinvest.ProxmoxVE.Admin.Module.SystemReport;

public class Module : ModuleBase
{
    public Module()
    {
        Keywords = "report,system,analysis,cluster,export,pdf,documentation,audit,overview,vm,node,storage";
        ModuleType = ModuleType.Application;
        Name = "System Report";
        Description = "Generate comprehensive cluster, VM, node and storage reports";
        Category = Categories.Utilities;
        Slug = "system-report";

        NavBar =
        [
            new(this,"Overview",string.Empty)
            {
                Render = new(typeof(Components.Overview)),
                Icon = PveAdminUIHelper.Icons.Overview
            },
            new(this,"Scans")
            {
                Render = new(typeof(Components.Scans)),
                Icon = PveAdminUIHelper.Icons.Scans
            }
        ];

        Link = new(this, Name, string.Empty)
        {
            Icon = "description",
            Render = NavBar.ToList()[0].Render
        };
    }

    protected override string PermissionBaseKey { get; } = "SystemReport";

    protected override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
       => services.AddDbContextFactoryPostgreSql<ModuleDbContext>("system_reports");

    public override Task DatabaseMaintenanceAsync(IServiceScope scope, DatabaseMaintenanceOperation operation)
        => scope.GetRequiredService<ModuleDbContext>().ExecuteMaintenanceAsync(operation);

    public override Task FixAsync(IServiceScope scope) => RunAsync(scope);

    protected override async Task RunAsync(IServiceScope scope)
        => await scope.MigrateDbAsync<ModuleDbContext>();

    //protected override async Task RefreshSettingsAsync(IServiceScope scope)
    //{
    //    await scope.GetEventNotificationService().PublishAsync(new DataChangedNotification());
    //    InitializeJob(scope);
    //}

    //protected override async Task FixAsync(IServiceScope scope)
    //{
    //    InitializeJob(scope);
    //    await Task.CompletedTask;
    //}

    //private static void InitializeJob(IServiceScope scope)
    //{
    //    var backgroundJobService = scope.GetJobService();
    //    var settingsService = scope.GetSettingsService();

    //    foreach (var item in settingsService.GetEnabledClustersSettings().Select(a => a.Name))
    //    {
    //        var settings = settingsService.GetForModule<Module, Settings>(item);
    //        backgroundJobService.ScheduleOrRemove<Job>(a => a.ScanAsync(settings.ClusterName),
    //                                         settings.CronExpression,
    //                                         settings.Enabled,
    //                                         settings.ClusterName);
    //    }
    //}
}
