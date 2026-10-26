/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Modularity;
using Corsinvest.ProxmoxVE.Admin.Core.Persistence;
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth.Permissions;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace Corsinvest.ProxmoxVE.Admin.Module.AutoSnap;

public class Module : ModuleBase
{
    public Module()
    {
        Keywords = "snapshot,automatic,scheduled,backup,retention,vm,ct,protection,snapshot management";
        ModuleType = ModuleType.Application;
        Name = "AutoSnap";
        Description = "Automated snapshot scheduling and management for VM/CT with retention policies";
        Category = Categories.Protection;
        Slug = "autosnap";
        HelpUrl = "modules/autosnap";

        NavBar =
        [
            new(this,"Overview",string.Empty)
            {
                Render = new(typeof(Components.Overview)),
                Icon = PveAdminUIHelper.Icons.Overview
            },
            new(this,"Jobs")
            {
                Render = new(typeof(Components.Jobs)),
                Icon = PveAdminUIHelper.Icons.Jobs
            },
            new(this,"Time line")
            {
                Render = new (typeof(Components.Timeline)),
                Icon = PveAdminUIHelper.Icons.Timeline
            },
            new(this,"Errors")
            {
                Render = new (typeof(Components.Errors)),
                Icon = PveAdminUIHelper.Icons.Errors
            },
            new(this,"Status")
            {
                Render = new (typeof(Components.Status),
                              new Dictionary<string, object> { [nameof(Components.Status.Style)] = "height: calc(100vh - 140px)" }),
                Icon = PveAdminUIHelper.Icons.Status
            }
        ];

        Widgets =
        [
            new(this,"Status")
            {
                ClusterWide = true,
                Description = "Autosnap Status",
                RenderInfo = new(typeof(Components.Widgets.Status)),
                Width = 3,
                Height = 5
            },
            new(this,"Size")
            {
                ClusterWide = true,
                Description = "Snapshot Size Over Time",
                RenderInfo = new(typeof(Components.Widgets.Size)),
                Width = 3,
                Height = 5
            },
            new(this,"Check")
            {
                ClusterWide = true,
                Description = "Failed Snapshots",
                RenderInfo = new(typeof(Components.Widgets.Check.Render)),
                RenderSettingsInfo = new(typeof(Components.Widgets.Check.CheckSettings),
                                         typeof(Components.Widgets.Check.RenderSettings)),
                Width = 3,
                Height = 5
            },
            new(this,"Info")
            {
                ClusterWide = true,
                Description = "Snapshot Statistics and Insights",
                RenderInfo = new(typeof(Components.Widgets.Info)),
                Width = 6,
                Height = 8
            },
            .. ModuleWidget.EnterprisePlaceholder(this, "Daily Status", "Success/Failed snapshots per day (last 14 days)", 6, 5)
        ];

        Link = new(this, Name, string.Empty)
        {
            Icon = PveAdminUIHelper.Icons.Snapshot,
            Render = NavBar.ToList()[0].Render
        };

        Roles =
        [
            new(Permissions.Job.Data.Permissions
                                    .CombineWith(Permissions.Job.Snap)
                                    .CombineWith(Permissions.Job.Purge)
                                    .CombineWith(Permissions.Status.Data)
                                    .CombineWith(Permissions.Status.Delete)
                                    .CombineWith(Permissions.Results.Data)
                                    .CombineWith(Permissions.Results.Delete))
        ];
    }

    public virtual Type? WebHookTabComponentType => null;

    protected override string PermissionBaseKey => Permissions.BaseName;

    protected override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        => AddSettings<Settings, Components.RenderSettings>(services)
            .AddDbContextFactoryPostgreSql<ModuleDbContext>("autosnap");

    public override IModuleMaintenance GetMaintenance(IServiceScope scope)
        => new PostgreSqlModuleMaintenance<ModuleDbContext>(scope);

    public override Task FixAsync(IServiceScope scope) => RunAsync(scope);

    protected override async Task RunAsync(IServiceScope scope)
    {
        await scope.MigrateDbAsync<ModuleDbContext>();
        await EnableWhereInUseAsync(scope);
        InitializeJob(scope);
    }

    // Once: the switch of the module did not stop the jobs before, so it is turned on where jobs are enabled
    private async Task EnableWhereInUseAsync(IServiceScope scope)
    {
        var settingsService = scope.GetSettingsService();
        if (settingsService.Get<ModuleSwitchMigration>().Done) { return; }

        using var db = scope.GetDbContext<ModuleDbContext>();
        foreach (var clusterName in db.Jobs.Where(a => a.Enabled).Select(a => a.ClusterName).Distinct().ToList())
        {
            var settings = settingsService.GetForModule<Module, Settings>(clusterName);
            if (settings.Enabled) { continue; }

            settings.Enabled = true;
            await settingsService.SetAsync(this, clusterName, settings);
        }

        await settingsService.SetAsync(new ModuleSwitchMigration { Done = true });
    }

    protected override Task RefreshSettingsAsync(IServiceScope scope)
    {
        InitializeJob(scope);
        return Task.CompletedTask;
    }

    private static void InitializeJob(IServiceScope scope)
    {
        var backgroundJobService = scope.GetBackgroundJobService();
        var settingsService = scope.GetSettingsService();
        using var db = scope.GetDbContext<ModuleDbContext>();

        // The jobs of a cluster that was deleted or disabled stay in the table: only the clusters where the module is on
        var clusterNames = settingsService.GetEnabledClustersSettings()
                                          .Select(a => a.Name)
                                          .Where(a => settingsService.GetForModule<Module, Settings>(a).Enabled)
                                          .ToHashSet();

        foreach (var item in ReadJobs(db, scope))
        {
            backgroundJobService.ScheduleOrRemove<Job>(a => a.SnapAsync(item.Id),
                                             item.CronExpression,
                                             item.Enabled && clusterNames.Contains(item.ClusterName),
                                             item.ClusterName,
                                             item.Id);
        }
    }

    // A job whose extended data cannot be read must not stop the start: it is skipped and the others are scheduled
    private static List<JobSchedule> ReadJobs(ModuleDbContext db, IServiceScope scope)
    {
        try
        {
            return [.. db.Jobs.AsNoTracking()];
        }
        catch (JsonException)
        {
            var logger = scope.GetLoggerFactory().CreateLogger<Module>();
            var ret = new List<JobSchedule>();
            foreach (var id in db.Jobs.Select(a => a.Id).ToList())
            {
                try
                {
                    ret.Add(db.Jobs.AsNoTracking().First(a => a.Id == id));
                }
                catch (JsonException ex)
                {
                    logger.LogError(ex, "AutoSnap: job {JobId} has extended data that cannot be read, it is not scheduled", id);
                }
            }

            return ret;
        }
    }

    public static class Permissions
    {
        public static string BaseName { get; } = "AutoSnap";

        public static class Job
        {
            public static PermissionsCrud Data { get; } = new(BaseName, nameof(Job), nameof(Data)) { WithRead = false };
            public static Permission Snap { get; } = new(Data.Prefix, nameof(Snap), "Snap");
            public static Permission Purge { get; } = new(Data.Prefix, nameof(Purge), "Purge Snapshots");
        }

        public static class Status
        {
            public static PermissionsRead Data { get; } = new(BaseName, nameof(Status), nameof(Data)) { WithRead = false };
            public static Permission Delete { get; } = new(Data.Prefix, nameof(Delete), "Delete");
        }

        public static class Results
        {
            public static PermissionsRead Data { get; } = new(BaseName, nameof(Results), nameof(Data)) { WithRead = false };
            public static Permission Delete { get; } = new(Data.Prefix, nameof(Delete), "Delete");
        }
    }
}
