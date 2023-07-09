/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.Domain.Repository;
using Corsinvest.AppHero.Core.Extensions;
using Corsinvest.AppHero.Core.Modularity;
using Corsinvest.AppHero.Core.Security.Auth.Permissions;
using Corsinvest.AppHero.Core.UI;
using Corsinvest.ProxmoxVE.Admin.Core.Modularity;
using Corsinvest.ProxmoxVE.Admin.NodeProtect.Components;
using Corsinvest.ProxmoxVE.Admin.NodeProtect.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Corsinvest.ProxmoxVE.Admin.NodeProtect;

public class Module : PveAdminModuleBase, IForceLoadModule
{
    public Module()
    {
        Authors = "Corsinvest Srl";
        Company = "Corsinvest Srl";
        Keywords = "IpAddress,Protect,Configuration,Options";
        Description = "Node Protect";
        InfoText = "Perform and schedule the saving of important files in case of node fault, for your Proxmox VE cluster";
        SetCategory(AdminModuleCategory.Safe);

        Link = new ModuleLink(this, Description)
        {
            Icon = Icons.Material.Filled.SafetyCheck,
            Render = typeof(RenderIndex)
        };

        Widgets = new[]
        {
            new ModuleWidget(this,"Status")
            {
                GroupName = Category,
                Render= typeof(Widget),
                Class = "mud-grid-item mud-grid-item-xs-12 mud-grid-item-sm-6 mud-grid-item-md-4 mud-grid-item-lg-4"
            }
        };

        Roles = new Role[]
        {
            new("",
                "",
                Permissions.DataGrid.Data.Permissions
                    .Union(new[]
                    {
                        Permissions.DataGrid.Execute,
                        Permissions.DataGrid.Delete,
                        Permissions.DataGrid.Download,
                        Permissions.DataGrid.Upload,
                        Permissions.DataGrid.ShowLog,
                    }))
        };

        UrlHelp += "#chapter_module_node_protect";
    }

    public override void ConfigureServices(IServiceCollection services, IConfiguration config)
        => AddOptions<Options, RenderOptions>(services, config)
            .AddDbContext<NodeProtectDbContext>(options => options.UseSqlite($"Data Source={Path.Combine(PathData, "db.db")}"))
            .AddRepository<NodeProtectDbContext, NodeProtectJobHistory>();

    public override async Task OnApplicationInitializationAsync(IHost host)
    {
        await Task.CompletedTask;
        host.DatabaseMigrate<NodeProtectDbContext>();
    }

    internal class Permissions
    {
        public class DataGrid
        {
            public static PermissionsRead Data { get; } = new($"{typeof(Module).FullName}.{nameof(DataGrid)}.{nameof(Data)}");

            public static Permission Execute { get; } = new($"{Data.Prefix}.{nameof(Execute)}", "Execute", Icons.Material.Filled.PlayArrow, UIColor.Success);
            public static Permission Delete { get; } = new($"{Data.Prefix}.{nameof(Delete)}", "Delete", Icons.Material.Filled.DeleteForever, UIColor.Error);
            public static Permission Download { get; } = new($"{Data.Prefix}.{nameof(Download)}", "Download", Icons.Material.Filled.Download);
            public static Permission Upload { get; } = new($"{Data.Prefix}.{nameof(Upload)}", "Upload", Icons.Material.Filled.Upload);
            public static Permission ShowLog { get; } = new($"{Data.Prefix}.{nameof(ShowLog)}", "Log", Icons.Material.Filled.Description);
        }
    }
}