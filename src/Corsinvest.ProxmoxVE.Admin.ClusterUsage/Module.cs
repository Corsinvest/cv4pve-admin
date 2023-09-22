/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.Domain.Repository;
using Corsinvest.AppHero.Core.Modularity;
using Corsinvest.AppHero.Core.Security.Auth.Permissions;
using Corsinvest.ProxmoxVE.Admin.ClusterUsage.Components;
using Corsinvest.ProxmoxVE.Admin.ClusterUsage.Persistence;
using Corsinvest.ProxmoxVE.Admin.Core.Modularity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Corsinvest.ProxmoxVE.Admin.ClusterUsage;

public class Module : PveAdminModuleBase, IForceLoadModule
{
    public Module()
    {
        Authors = "Corsinvest Srl";
        Company = "Corsinvest Srl";
        Keywords = "Cluster,CPU,Ram,Memory,Storage,Size,Size";
        Description = "Cluster Usage";
        InfoText = "Control how each component of your Proxmox VE cluster occupies resources";
        SetCategory(AdminModuleCategory.Health);

        Link = new ModuleLink(this, Description)
        {
            Icon = Icons.Material.Filled.DataUsage,
            Render = typeof(RenderIndex)
        };

        Roles = new Role[]
        {
            new("",
                "",
                Permissions.Costs.Data.Permissions
                    .Union(new []
                    {
                        Permissions.Costs.Scan
                    })
                    .Union(Permissions.Vms.Data.Permissions)
                    .Union(Permissions.Storages.Data.Permissions))
        };

        UrlHelp += "#chapter_module_cluster_usage";
    }

    public override void ConfigureServices(IServiceCollection services, IConfiguration config)
        => AddOptions<Options, RenderOptions>(services, config)
            .AddDbContext<ClusterUsageDbContext>(options => options.UseSqlite($"Data Source={Path.Combine(PathData, "db.db")}"))
            .AddRepositories<ClusterUsageDbContext>(new[] { typeof(DataVm), typeof(DataVmStorage) });

    public override async Task OnApplicationInitializationAsync(IHost host)
    {
        await Task.CompletedTask;
        host.DatabaseMigrate<ClusterUsageDbContext>();
    }

    public class Permissions
    {
        public class Costs
        {
            public static PermissionsRead Data { get; } = new($"{typeof(Module).FullName}.{nameof(Costs)}.{nameof(Data)}");
            public static Permission Scan { get; } = new($"{Data.Prefix}.{nameof(Scan)}", "Scan", Icons.Material.Filled.PlayArrow, UIColor.Success);
        }

        public class Vms
        {
            public static PermissionsRead Data { get; } = new($"{typeof(Module).FullName}.{nameof(Vms)}.{nameof(Data)}");
        }

        public class Storages
        {
            public static PermissionsRead Data { get; } = new($"{typeof(Module).FullName}.{nameof(Storages)}.{nameof(Data)}");
        }
    }
}