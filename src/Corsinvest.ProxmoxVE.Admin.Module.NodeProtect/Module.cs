/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Persistence;
using Corsinvest.ProxmoxVE.Admin.Module.NodeProtect.Folder.Helpers;
using Corsinvest.ProxmoxVE.Admin.Module.NodeProtect.Models;
using Corsinvest.ProxmoxVE.Admin.Module.NodeProtect.Persistence;

namespace Corsinvest.ProxmoxVE.Admin.Module.NodeProtect;

public class Module : ModuleBase
{
    public Module()
    {
        Keywords = "node,configuration,backup,protection,restore,git,versioning";
        ModuleType = ModuleType.Application;
        Name = "Node Protect";
        Description = "Automated node configuration backup, versioning and restore";
        Category = Categories.Protection;
        Slug = "node-protect";

        var navBar = new List<ModuleLinkBase>()
        {
           new(this,"Overview", string.Empty)
           {
                Render = new(typeof(Components.Overview)),
                Icon = PveAdminUIHelper.Icons.Overview
           },
        };
        navBar.AddRange(GetProviders().Select(a => new ModuleLinkBase(this, a.Name)
        {
            Render = a.Render,
            Icon = a.Icon
        }));

        NavBar = navBar;

        Link = new(this, Name, string.Empty)
        {
            Icon = "safety_check",
            Render = NavBar.ToList()[0].Render
        };

        Widgets =
        [
            new(this, "Folder Size")
            {
                Description = "NodeProtect Folder Size",
                RenderInfo = new(typeof(Folder.Components.Widgets.Size)),
                Width = 3,
                Height = 5
            }
        ];
    }

    protected override string PermissionBaseKey { get; } = "NodeProtect";

    protected override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        AddSettings<Settings, Components.RenderSettings>(services);
        services.AddDbContextFactoryPostgreSql<ModuleDbContext>("node_protect");

        FolderHelper.ConfigureService();
    }

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

    private void InitializeJob(IServiceScope scope)
    {
        var backgroundJobService = scope.GetBackgroundJobService();
        var settingsService = scope.GetSettingsService();

        foreach (var item in settingsService.GetEnabledClustersSettings().Select(a => a.Name))
        {
            var settings = settingsService.GetForModule<Module, Settings>(item);

            backgroundJobService.ScheduleOrRemove<Folder.Job>(a => a.BackupAsync(settings.ClusterName),
                                                                       settings.CronExpression,
                                                                       settings.Enabled && settings.Folder.Enabled,
                                                                       settings.ClusterName);

            InitializeJob(backgroundJobService, settings);
        }
    }

    protected virtual void InitializeJob(IBackgroundJobService backgroundJobService, Settings settings) { }

    public virtual IEnumerable<Provider> GetProviders() =>
    [
        new("Folder", new(typeof(Folder.Components.Render)), new(typeof(Folder.Components.RenderSettings)),"folder_zip"),
        new("Git", new(typeof(Core.Components.SubscriptionRequired)),new(typeof(Core.Components.SubscriptionRequired)),"commit")
        //Icon="󰊢" class="mdi"
    ];

}
