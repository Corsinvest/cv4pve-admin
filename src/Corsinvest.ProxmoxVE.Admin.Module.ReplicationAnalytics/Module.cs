/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Helpers;
using Corsinvest.ProxmoxVE.Admin.Core.Modularity;
using Corsinvest.ProxmoxVE.Admin.Core.Persistence;
using Microsoft.Extensions.Configuration;

namespace Corsinvest.ProxmoxVE.Admin.Module.ReplicationAnalytics;

public class Module : ModuleBase
{
    public Module()
    {
        Keywords = "replication,monitoring,analysis,sync,insights,trends,data transfer";
        ModuleType = ModuleType.Application;
        Name = "Replication Analytics";
        Description = "Replication job monitoring, analysis and synchronization insights";
        Category = Categories.Health;
        Slug = "replication-analytics";
        HelpUrl = "modules/replication-analytics";

        NavBar =
        [
            new(this,"Overview", string.Empty)
            {
                Render = new(typeof(Components.Overview)),
                Icon = PveAdminUIHelper.Icons.Overview
            },
            new(this,"Replications")
            {
                Render = new(typeof(Components.Replications)),
                Icon = PveAdminUIHelper.Icons.Replication
            },
            new(this,"Scheduled")
            {
                Render = new(typeof(Components.Scheduled)),
                Icon = PveAdminUIHelper.Icons.Schedule
            },
            new(this,"Trends")
            {
                Render = new(typeof(Components.Trends)),
                Icon = PveAdminUIHelper.Icons.Trends
            }
        ];

        Link = new(this, Name, string.Empty)
        {
            Icon = PveAdminUIHelper.Icons.Replication,
            Render = NavBar.ToList()[0].Render
        };

        Widgets =
        [
            new(this,"Status")
            {
                Description = "Replication Analytics Status",
                RenderInfo = new(typeof(Components.Widgets.Status)),
                Width = 3,
                Height = 5
            },
            new(this,"Size")
            {
                Description = "Replication Analytics Size",
                RenderInfo = new(typeof(Components.Widgets.Size)),
                Width = 3,
                Height = 5
            },
            new(this,"Check")
            {
                Description = "Replication Check",
                RenderInfo = new(typeof(Components.Widgets.Check.Render)),
                RenderSettingsInfo = new(typeof(Components.Widgets.Check.CheckSettings),
                                         typeof(Components.Widgets.Check.RenderSettings)),
                Width = 3,
                Height = 5
            },
            new(this,"Info")
            {
                Description = "Replication Analytics Overview",
                RenderInfo = new(typeof(Components.Widgets.Info)),
                Width = 6,
                Height = 8
            }
        ];
    }

    protected override string PermissionBaseKey { get; } = "ReplicationAnalytics";

    protected override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        => AddSettings<Settings, Components.RenderSettings>(services)
            .AddDbContextFactoryPostgreSql<ModuleDbContext>("replication_insights");

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
