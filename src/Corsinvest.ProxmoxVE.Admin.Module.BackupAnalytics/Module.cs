/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Helpers;
using Corsinvest.ProxmoxVE.Admin.Core.Modularity;
using Corsinvest.ProxmoxVE.Admin.Core.Persistence;
using Microsoft.Extensions.Configuration;

namespace Corsinvest.ProxmoxVE.Admin.Module.BackupAnalytics;

public class Module : ModuleBase
{
    public Module()
    {
        Keywords = "backup,analysis,monitoring,insights,jobs,storage,trends,protection,scheduled";
        ModuleType = ModuleType.Application;
        Name = "Backup Analytics";
        Description = "Comprehensive backup job analysis, monitoring and trend insights";
        Category = Categories.Health;
        Slug = "backup-analytics";

        NavBar =
        [
            new(this,"Overview", string.Empty)
            {
                Render = new(typeof(Components.Overview)),
                Icon = PveAdminUIHelper.Icons.Overview
            },
            new(this,"Backups")
            {
                Render = new(typeof(Components.Backups)),
                Icon = PveAdminUIHelper.Icons.Backup
            },
            new(this,"Scheduled")
            {
                Render = new(typeof(Components.Scheduled)),
                Icon = PveAdminUIHelper.Icons.Schedule
            },
            new(this,"Unprotected Guests")
            {
                Render = new(typeof(Components.UnprotectedGuests)),
                Icon = PveAdminUIHelper.Icons.Warning
            },
            new(this,"Unprotected Disks")
            {
                Render = new (typeof(Components.UnprotectedDisks)),
                Icon = PveAdminUIHelper.Icons.Errors
            },
            new(this,"Backup in line")
            {
                Render = new(typeof(Components.BackupInline)),
                Icon = "list"
            },
            new(this,"Trends")
            {
                Render = new(typeof(Components.Trends)),
                Icon = PveAdminUIHelper.Icons.Trends
            },
        ];

        Link = new(this, Name, string.Empty)
        {
            Icon = PveAdminUIHelper.Icons.Backup,
            Render = NavBar.ToList()[0].Render
        };

        Widgets =
        [
            new(this,"Status")
            {
                Description = "Status",
                RenderInfo = new(typeof(Components.Widgets.Status)),
                Width = 3,
                Height = 5
            },
            new(this,"Size")
            {
                Description = "Size",
                RenderInfo = new(typeof(Components.Widgets.Size)),
                Width = 3,
                Height = 5
            },
            new(this,"Check")
            {
                Description = "Check",
                RenderInfo = new(typeof(Components.Widgets.Check.Render)),
                RenderSettingsInfo = new(typeof(Components.Widgets.Check.CheckSettings),
                                         typeof(Components.Widgets.Check.RenderSettings)),
                Width = 3,
                Height = 5
            },
            new(this,"Info")
            {
                Description = "Info",
                RenderInfo = new(typeof(Components.Widgets.Info)),
                Width = 6,
                Height = 8
            }
        ];
    }

    protected override string PermissionBaseKey { get; } = "BackupAnalytics";

    protected override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        => AddSettings<Settings, Components.RenderSettings>(services)
            .AddDbContextFactoryPostgreSql<ModuleDbContext>("backup_insights");

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
