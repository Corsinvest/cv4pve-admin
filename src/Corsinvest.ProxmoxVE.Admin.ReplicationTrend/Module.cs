/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.Modularity;
using Corsinvest.AppHero.Core.Security.Auth.Permissions;
using Corsinvest.AppHero.Core.UI;
using Corsinvest.ProxmoxVE.Admin.Core.Modularity;
using Corsinvest.ProxmoxVE.Admin.ReplicationTrend.Components;
using Corsinvest.ProxmoxVE.Admin.ReplicationTrend.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Corsinvest.ProxmoxVE.Admin.ReplicationTrend;

public class Module : PveAdminModuleBase, IForceLoadModule
{
    public Module()
    {
        Authors = "Corsinvest Srl";
        Company = "Corsinvest Srl";
        Keywords = "Replication,Sync,Trend";
        Description = "Replication Trend";
        InfoText = "Check the progress of the replication set by Proxmox VE and check status, show more info";
        SetCategory(AdminModuleCategory.Health);

        Link = new ModuleLink(this, Description)
        {
            Icon = Icons.Material.Filled.Repeat,
            Render = typeof(RenderIndex)
        };

        Roles = new Role[]
        {
            new("",
                "",
                Permissions.Job.Data.Permissions
                    .Union(new []
                    {
                        Permissions.Job.Scan,
                        Permissions.Job.ShowLog,
                    }))
        };

        Widgets = new[]
        {
            new ModuleWidget(this,"Status")
            {
                GroupName = Category,
                Render = typeof(RenderWidget),
                Class = "mud-grid-item mud-grid-item-xs-12 mud-grid-item-sm-6 mud-grid-item-md-4 mud-grid-item-lg-4"
            }
        };

        UrlHelp += "#chapter_module_replication_trend";
    }

    public override void ConfigureServices(IServiceCollection services, IConfiguration config)
        => AddOptions<Options, RenderOptions>(services, config)
            .AddDbContext<ReplicationTrendDbContext>(options => options.UseSqlite($"Data Source={Path.Combine(PathData, "db.db")}"))
            .AddRepositories<ReplicationTrendDbContext>(new[] { typeof(ReplicationResult) });

    public override async Task OnApplicationInitializationAsync(IHost host)
    {
        await Task.CompletedTask;
        host.DatabaseMigrate<ReplicationTrendDbContext>();
    }

    public class Permissions
    {
        public static class Job
        {
            public static PermissionsRead Data { get; } = new($"{typeof(Module).FullName}.{nameof(Job)}.{nameof(Data)}");
            public static Permission Scan { get; } = new($"{Data.Prefix}.{nameof(Scan)}", "Scan replication Proxmox VE", Icons.Material.Filled.PlayArrow, UIColor.Success);
            public static Permission ShowLog { get; } = new($"{Data.Prefix}.{nameof(ShowLog)}", "Show log", Icons.Material.Filled.ListAlt, UIColor.Primary);
        }
    }
}