/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.Domain.Repository;
using Corsinvest.AppHero.Core.Modularity;
using Corsinvest.AppHero.Core.Security.Auth.Permissions;
using Corsinvest.AppHero.Core.UI;
using Corsinvest.ProxmoxVE.Admin.Core.Modularity;
using Corsinvest.ProxmoxVE.Admin.VzDumpTrend.Components;
using Corsinvest.ProxmoxVE.Admin.VzDumpTrend.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Corsinvest.ProxmoxVE.Admin.VzDumpTrend;

public class Module : PveAdminModuleBase, IForceLoadModule
{
    public Module()
    {
        Authors = "Corsinvest Srl";
        Company = "Corsinvest Srl";
        Keywords = "VzDump,Backup,Trend";
        Description = "VzDump Trend";
        InfoText = "Check the progress of the vzdump backup set by Proxmox VE and check status, show more info";
        SetCategory(ModuleCategory.Health);

        Link = new ModuleLink(this, Description)
        {
            Icon = PveBlazorHelper.Icons.Backup,
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
                        Permissions.Job.ShowLogFull,
                        Permissions.Job.ShowLogVm
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

        UrlHelp += "#chapter_module_vzdump_trend";
    }

    public override void ConfigureServices(IServiceCollection services, IConfiguration config)
        => AddOptions<Options, RenderOptions>(services, config)
            .AddDbContext<VzDumpTrendDbContext>(options => options.UseSqlite($"Data Source={Path.Combine(PathData, "db.db")}"))
            .AddRepositories<VzDumpTrendDbContext>(new[] { typeof(VzDumpDetail), typeof(VzDumpTask) });
    public override async Task OnApplicationInitializationAsync(IHost host)
    {
        await Task.CompletedTask;
        host.DatabaseMigrate<VzDumpTrendDbContext>();
    }

    public class Permissions
    {
        public static class Job
        {
            public static PermissionsRead Data { get; } = new($"{typeof(Module).FullName}.{nameof(Job)}.{nameof(Data)}");
            public static Permission Scan { get; } = new($"{Data.Prefix}.{nameof(Scan)}", "Scan backups Proxmox VE", Icons.Material.Filled.PlayArrow, UIColor.Success);
            public static Permission ShowLogVm { get; } = new($"{Data.Prefix}.{nameof(ShowLogVm)}", "Show log VM/CT", Icons.Material.Filled.ListAlt, UIColor.Primary);
            public static Permission ShowLogFull { get; } = new($"{Data.Prefix}.{nameof(ShowLogFull)}", "Show full log", Icons.Material.Filled.ListAlt, UIColor.Tertiary);
        }
    }
}