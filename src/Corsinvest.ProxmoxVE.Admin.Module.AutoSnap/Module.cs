/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Modularity;
using Corsinvest.ProxmoxVE.Admin.Core.Persistence;
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth.Permissions;
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
                Render = new (typeof(Components.Status)),
                Icon = PveAdminUIHelper.Icons.Status
            }
        ];

        Widgets =
        [
            new(this,"Status")
            {
                Description = "Autosnap Status",
                RenderInfo = new(typeof(Components.Widgets.Status)),
                Width = 3,
                Height = 5
            },
            new(this,"Size")
            {
                Description = "Snapshot Size Over Time",
                RenderInfo = new(typeof(Components.Widgets.Size)),
                Width = 3,
                Height = 5
            },
            new(this,"Check")
            {
                Description = "Failed Snapshots",
                RenderInfo = new(typeof(Components.Widgets.Check.Render)),
                RenderSettingsInfo = new(typeof(Components.Widgets.Check.CheckSettings),
                                         typeof(Components.Widgets.Check.RenderSettings)),
                Width = 3,
                Height = 5
            },
            new(this,"Info")
            {
                Description = "Snapshot Statistics and Insights",
                RenderInfo = new(typeof(Components.Widgets.Info)),
                Width = 6,
                Height = 8
            }
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
        using var db = scope.GetDbContext<ModuleDbContext>();

        foreach (var item in db.Jobs)
        {
            backgroundJobService.ScheduleOrRemove<Job>(a => a.SnapAsync(item.Id),
                                             item.CronExpression,
                                             item.Enabled,
                                             item.ClusterName,
                                             item.Id);
        }
    }

    public static class Permissions
    {
        public static string BaseName { get; } = "AutoSnap";

        public static class Job
        {
            public static PermissionsCrud Data { get; } = new(BaseName, nameof(Job), nameof(Data));
            public static Permission Snap { get; } = new(Data.Prefix, nameof(Snap), "Snap");
            public static Permission Purge { get; } = new(Data.Prefix, nameof(Purge), "Purge Snapshots");
        }

        public static class Status
        {
            public static PermissionsRead Data { get; } = new(BaseName, nameof(Status), nameof(Data));
            public static Permission Delete { get; } = new(Data.Prefix, nameof(Delete), "Delete");
        }

        public static class Results
        {
            public static PermissionsRead Data { get; } = new(BaseName, nameof(Results), nameof(Data));
            public static Permission Delete { get; } = new(Data.Prefix, nameof(Delete), "Delete");
        }
    }
}
