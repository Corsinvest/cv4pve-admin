/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Persistence;
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth.Permissions;
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
        HelpUrl = "modules/node-protect";

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

        Roles =
        [
            new(Permissions.FolderBackup.Data.Permissions
                                             .CombineWith(Permissions.FolderBackup.Backup)
                                             .CombineWith(Permissions.FolderBackup.Delete)
                                             .CombineWith(Permissions.FolderBackup.Download)
                                             .CombineWith(Permissions.Git.Data)
                                             .CombineWith(Permissions.Git.Push)
                                             .CombineWith(Permissions.Git.Reset)
                                             .CombineWith(Permissions.Git.Sync)
                                             .CombineWith(Permissions.Git.Download))
        ];
    }

    public ModuleLinkBase? GetLinkByProvider(string name)
        => NavBar.FirstOrDefault(a => a.Text.Equals(name, StringComparison.CurrentCultureIgnoreCase));

    protected override string PermissionBaseKey => Permissions.BaseName;

    public static class Permissions
    {
        public static string BaseName { get; } = "NodeProtect";

        public static class FolderBackup
        {
            public static PermissionsRead Data { get; } = new(BaseName, nameof(FolderBackup), nameof(Data));
            public static Permission Backup { get; } = new(Data.Prefix, nameof(Backup), "Backup");
            public static Permission Delete { get; } = new(Data.Prefix, nameof(Delete), "Delete");
            public static Permission Download { get; } = new(Data.Prefix, nameof(Download), "Download");
        }

        // Checked by the Git provider of the Enterprise edition
        public static class Git
        {
            public static PermissionsRead Data { get; } = new(BaseName, nameof(Git), nameof(Data));
            public static Permission Push { get; } = new(Data.Prefix, nameof(Push), "Push");
            public static Permission Reset { get; } = new(Data.Prefix, nameof(Reset), "Reset repository");
            public static Permission Sync { get; } = new(Data.Prefix, nameof(Sync), "Sync from remote");
            public static Permission Download { get; } = new(Data.Prefix, nameof(Download), "Download");
        }
    }

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

    public override IModuleMaintenance GetMaintenance(IServiceScope scope)
        => new PostgreSqlModuleMaintenance<ModuleDbContext>(scope);

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

    /// <summary>The Git provider's name, shared so the Enterprise edition can replace this
    /// placeholder entry instead of adding a second one beside it. A property rather than a
    /// const: a const is inlined into the Enterprise assembly, which would then keep filtering
    /// on the old name until it is rebuilt — bringing the duplicate back in silence.</summary>
    public static string GitProviderName { get; } = "Git";

    public virtual IEnumerable<Provider> GetProviders() =>
    [
        new("Folder", new(typeof(Folder.Components.Render)), new(typeof(Folder.Components.RenderSettings)),"folder_zip"),
        new(GitProviderName, new(typeof(Core.Components.SubscriptionRequired)),new(typeof(Core.Components.SubscriptionRequired)),"commit")
        //Icon="󰊢" class="mdi"
    ];

}
