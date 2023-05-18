/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.Domain.Repository;
using Corsinvest.AppHero.Core.Extensions;
using Corsinvest.ProxmoxVE.Admin.AutoSnap.Components;
using Corsinvest.ProxmoxVE.Admin.AutoSnap.Models;
using Corsinvest.ProxmoxVE.Admin.AutoSnap.Persistence;
using Corsinvest.ProxmoxVE.Admin.Core.Modularity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Corsinvest.ProxmoxVE.Admin.AutoSnap;

public class Module : PveAdminModuleBase, IForceLoadModule
{
    public Module()
    {
        Authors = "Corsinvest Srl";
        Company = "Corsinvest Srl";
        Keywords = "Snapshot,Automatic";
        InfoText = "Take, schedule and manage automatic snapshots for VM/CT in your Proxmox VE cluster";
        SetCategory(ModuleCategory.Safe);

        Description = "Auto Snapshot";

        Link = new ModuleLink(this, Description)
        {
            Icon = PveBlazorHelper.Icons.Snapshot,
            Render = typeof(RenderIndex),
        };

        Roles = new Role[]
        {
            new("",
                "",
                Permissions.Job.Data.Permissions.Union(new []
                {
                    Permissions.Job.Snap,
                    Permissions.Job.Clean
                }).Union(Permissions.Status.Data.Permissions.Union(new []
                {
                    Permissions.Status.Delete
                })).Union(Permissions.History.Data.Permissions.Union(new []
                {
                    Permissions.History.ShowLog
                })))
        };

        Widgets = new[]
        {
            new ModuleWidget(this,"99Info")
            {
                Render= typeof(RenderWidgetInfo),
                ShowDefaultHeader = false,
                Class = "mud-grid-item mud-grid-item-xs-12 mud-grid-item-sm-4 mud-grid-item-md-2 mud-grid-item-lg-2"
            },

            new ModuleWidget(this,"Status")
            {
                GroupName = Category,
                Render= typeof(RenderWidget),
                Class = "mud-grid-item mud-grid-item-xs-12 mud-grid-item-sm-6 mud-grid-item-md-4 mud-grid-item-lg-4"
            }
        };

        UrlHelp += "#chapter_module_autosnap";

        PvePermissionRequired = new[] { "VM.Audit", "VM.Snapshot", "Datastore.Audit", "Pool.Allocate" };
    }

    public override void ConfigureServices(IServiceCollection services, IConfiguration config)
        => AddOptions<Options, RenderOptions>(services, config)
            .AddDbContext<AutoSnapDbContext>(options => options.UseSqlite($"Data Source={Path.Combine(PathData, "db.db")}"))
            .AddRepositories<AutoSnapDbContext>(new[]
                                               {
                                                    typeof(AutoSnapJob),
                                                    typeof(AutoSnapJobHistory),
                                                    typeof(AutoSnapJobHook)
                                               });
    public override async Task OnApplicationInitializationAsync(IHost host)
    {
        await Task.CompletedTask;
        host.DatabaseMigrate<AutoSnapDbContext>();
    }

    public class Permissions
    {
        public class Job
        {
            public static PermissionsCrud Data { get; } = new($"{typeof(Module).FullName}.{nameof(Job)}.{nameof(Data)}");
            public static Permission Snap { get; } = new($"{Data.Prefix}.{nameof(Snap)}", "Snap", Icons.Material.Filled.PlayArrow, UIColor.Success);
            public static Permission Clean { get; } = new($"{Data.Prefix}.{nameof(Clean)}", "Clean", Icons.Material.Filled.CleaningServices, UIColor.Warning);
        }

        public class Status
        {
            public static PermissionsRead Data { get; } = new($"{nameof(AutoSnap)}.{nameof(Status)}.{nameof(Data)}");
            public static Permission Delete { get; } = new($"{Data.Prefix}.{nameof(Delete)}", "Delete", Icons.Material.Filled.DeleteForever, UIColor.Error);
        }

        public class History
        {
            public static PermissionsRead Data { get; } = new($"{nameof(AutoSnap)}.{nameof(History)}.{nameof(Data)}");
            public static Permission ShowLog { get; } = new($"{Data.Prefix}.{nameof(ShowLog)}", "Show log", Icons.Material.Filled.Description, UIColor.Primary);
        }
    }
}