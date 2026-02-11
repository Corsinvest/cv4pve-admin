using Corsinvest.ProxmoxVE.Admin.Core.Helpers;
using Corsinvest.ProxmoxVE.Admin.Core.Modularity;
using Corsinvest.ProxmoxVE.Admin.Core.Persistence;
using Corsinvest.ProxmoxVE.Admin.Module.Diagnostic.Services;
using Microsoft.Extensions.Configuration;

namespace Corsinvest.ProxmoxVE.Admin.Module.Diagnostic;

public class Module : ModuleBase
{
    public Module()
    {
        Keywords = "diagnostic,health,troubleshoot,scan,issues,errors,analysis,cluster health";
        ModuleType = ModuleType.Application;
        Name = "Diagnostic";
        Description = "Automated cluster health checks, diagnostics and issue detection";
        Category = Categories.Health;
        Slug = "diagnostic";

        NavBar =
        [
            new(this,"Overview", string.Empty)
            {
                Render = new(typeof(Components.Overview)),
                Icon = PveAdminUIHelper.Icons.Overview
            },
            new(this,"Scans")
            {
                Render = new(typeof(Components.Scans)),
                Icon = PveAdminUIHelper.Icons.Scans
            },
            new(this,"Ignored Issues")
            {
                Render = new(typeof(Components.Issues)),
                Icon = "block"
            }
        ];

        Link = new(this, Name, string.Empty)
        {
            Icon = "stethoscope",
            Render = NavBar.ToList()[0].Render
        };

        Widgets =
        [
            new(this,"Status")
            {
                Description = "Diagnostic Status",
                RenderInfo = new(typeof(Components.Widgets.Status)),
                Width = 3,
                Height = 5
            },
            new(this,"Check")
            {
                Description = "Diagnostic Issues Check",
                RenderInfo = new(typeof(Components.Widgets.Check)),
                Width = 3,
                Height = 5
            }
        ];
    }

    protected override string PermissionBaseKey { get; } = "Diagnostic";

    protected override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        => AddSettings<Settings, Components.RenderSettings>(services)
            .AddDbContextFactoryPostgreSql<ModuleDbContext>("diagnostic")
            .AddScoped<IDiagnosticService, DiagnosticService>();

    protected override async Task RefreshSettingsAsync(IServiceScope scope)
    {
        await scope.GetEventNotificationService().PublishAsync(new DataChangedNotification());
        InitializeJob(scope);
    }

    public override Task DatabaseMaintenanceAsync(IServiceScope scope, DatabaseMaintenanceOperation operation)
        => scope.GetRequiredService<ModuleDbContext>().ExecuteMaintenanceAsync(operation);

    public override Task FixAsync(IServiceScope scope) => RunAsync(scope);

    protected override async Task RunAsync(IServiceScope scope)
    {
        await scope.MigrateDbAsync<ModuleDbContext>();
        InitializeJob(scope);
    }

    private static void InitializeJob(IServiceScope scope)
    {
        var backgroundJobService = scope.GetBackgroundJobService();
        var settingsService = scope.GetSettingsService();

        foreach (var item in settingsService.GetEnabledClustersSettings().Select(a => a.Name))
        {
            var settings = settingsService.GetForModule<Module, Settings>(item);
            backgroundJobService.ScheduleOrRemove<Job>(a => a.ScanAsync(settings.ClusterName),
                                             settings.CronExpression,
                                             settings.Enabled,
                                             settings.ClusterName);
        }
    }
}
