/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.Modularity;
using Corsinvest.AppHero.Core.Security.Auth.Permissions;
using Corsinvest.AppHero.Core.UI;
using Corsinvest.ProxmoxVE.Admin.Core.Modularity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Corsinvest.ProxmoxVE.Admin.Core.Options;

public class Module : PveAdminModuleBase, IForceLoadModule
{
    public Module()
    {
        Authors = "Corsinvest Srl";
        Company = "Corsinvest Srl";
        Keywords = "Options,Proxmox VE,Pve";
        SetCategory(AdminModuleCategory.System);
        Icon = PveAdminHelper.SvgIconApp;
        Type = ModuleType.Service;
        Description = "cv4pve-Admin";

        Roles = new Role[]
        {
            new("",
                "",
                Permissions.Clusters.Data.Permissions.Union(
                Permissions.Nodes.Data.Permissions.Union(new []
                {
                    Permissions.Nodes.FindNewNodes
                })))
        };
    }

    public override void ConfigureServices(IServiceCollection services, IConfiguration config)
        => AddOptions<AdminOptions, RenderAdminOptions>(services, config);

    public class Permissions
    {
        public class Clusters
        {
            public static PermissionsCrud Data { get; } = new($"{typeof(Module).FullName}.{nameof(Clusters)}.{nameof(Data)}");
        }

        public class Nodes
        {
            public static PermissionsCrud Data { get; } = new($"{typeof(Module).FullName}.{nameof(Nodes)}.{nameof(Data)}");
            public static Permission FindNewNodes { get; } = new($"{Data.Prefix}.{nameof(FindNewNodes)}", "Nodes finder", Icons.Material.Filled.TravelExplore, UIColor.Info);
        }
    }
}