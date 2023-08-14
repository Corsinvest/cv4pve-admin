/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.Modularity;
using Corsinvest.AppHero.Core.Security.Auth.Permissions;
using Corsinvest.AppHero.Core.UI;
using Corsinvest.ProxmoxVE.Admin.Core.Modularity;
using Corsinvest.ProxmoxVE.Admin.DisksStatus.Components;

namespace Corsinvest.ProxmoxVE.Admin.DisksStatus;

public class Module : PveAdminModuleBase, IForceLoadModule
{
    public Module()
    {
        Authors = "Corsinvest Srl";
        Company = "Corsinvest Srl";
        Keywords = "Node,Host,Disk,Blink";
        Description = "Disks Status";
        InfoText = "Check the status of the disks in your Proxmox VE cluster";
        SetCategory(AdminModuleCategory.Utilities);

        Link = new ModuleLink(this, Description)
        {
            Icon = Icons.Material.Filled.Storage,
            Render = typeof(RenderIndex)
        };

        Roles = new Role[]
        {
            new("",
                "",
                Permissions.DataGrid.Data.Permissions
                    .Union(new[]
                    {
                        Permissions.DataGrid.BlinkLed
                    }))
        };

        UrlHelp += "#chapter_module_disks_status";
    }

    public class Permissions
    {
        public class DataGrid
        {
            public static PermissionsRead Data { get; } = new($"{typeof(Module).FullName}.{nameof(DataGrid)}.{nameof(Data)}");
            public static Permission BlinkLed { get; } = new($"{Data.Prefix}.{nameof(BlinkLed)}",
                                                             "Blink led",
                                                             Icons.Material.Filled.WbTwilight, UIColor.Default);
        }
    }
}