/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Helpers;
using Corsinvest.ProxmoxVE.Admin.Module.Updater.Services;
using Microsoft.Extensions.Configuration;

namespace Corsinvest.ProxmoxVE.Admin.Module.Updater;

public class Module : ModuleBase
{
    public Module()
    {
        Keywords = "update,upgrade,patches,security,packages,scheduled,scan,maintenance";
        ModuleType = ModuleType.Application;
        Name = "Update Manager";
        Description = "System updates scanning and management";
        Category = Categories.Health;
        Slug = "update-manager";

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
        //    new(this,"Cluster")
        //    {
        //////                Render = new (typeof(Components.Nodes)),
        //        Icon = PveAdminUIHelper.Icons.Cluster
        //    },
        ];

        Link = new(this, Name, string.Empty)
        {
            Icon = "update",
            Render = NavBar.ToList()[0].Render
        };

        Widgets =
        [
            new(this,"Status")
            {
                Description = "Updater Status",
                RenderInfo = new(typeof(Components.Widgets.Status)),
                Width = 3,
                Height = 5
            }
        ];
    }

    protected override string PermissionBaseKey { get; } = "UpdateManager";

    protected override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        => AddSettings<Settings, Components.RenderSettings>(services)
            .AddScoped<IUpdaterService, UpdaterService>();

    protected override async Task RefreshSettingsAsync(IServiceScope scope)
    {
        await scope.GetEventNotificationService().PublishAsync(new DataChangedNotification());

        InitializeJob(scope);
        await Task.CompletedTask;
    }

    public override Task FixAsync(IServiceScope scope) => RunAsync(scope);

    protected override async Task RunAsync(IServiceScope scope)
    {
        InitializeJob(scope);
        await Task.CompletedTask;
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
