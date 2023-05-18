/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.Modularity;
using Corsinvest.AppHero.Core.Security.Auth.Permissions;
using Corsinvest.AppHero.Core.UI;
using Corsinvest.ProxmoxVE.Admin.Core.Modularity;

namespace Corsinvest.ProxmoxVE.Admin.NodeFreeMemory;

public class Module : PveAdminModuleBase, IForceLoadModule
{
    public Module()
    {
        Authors = "Corsinvest Srl";
        Company = "Corsinvest Srl";
        Keywords = "Node,Host,Free,Memory";
        Description = "Free Memory Node";
        SetCategory(ModuleCategory.Utilities);
        InfoText = "Free up the node's memory space in case of error: \"out of memory\" or \"kvm: failed to initialize KVM: Cannot allocate memory\"";

        Link = new ModuleLink(this, Description)
        {
            Icon = Icons.Material.Filled.Memory,
            Render = typeof(RenderIndex)
        };

        Roles = new Role[]
        {
            new("",
                "",
                Permissions.DataGrid.Data.Permissions
                    .Union(new[]
                    {
                        Permissions.DataGrid.FreeMemory
                    }))
        };

        UrlHelp += "#chapter_module_node_free_memory";
    }

    public class Permissions
    {
        public class DataGrid
        {
            public static PermissionsRead Data { get; } = new($"{typeof(Module).FullName}.{nameof(DataGrid)}.{nameof(Data)}");
            public static Permission FreeMemory { get; } = new($"{Data.Prefix}.{nameof(FreeMemory)}",
                                                               "Free Memory",
                                                               Icons.Material.Filled.CleaningServices,
                                                               UIColor.Error);
        }
    }
}