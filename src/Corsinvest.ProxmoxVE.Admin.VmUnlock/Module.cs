/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.Modularity;
using Corsinvest.AppHero.Core.Security.Auth.Permissions;
using Corsinvest.AppHero.Core.UI;
using Corsinvest.ProxmoxVE.Admin.Core.Modularity;

namespace Corsinvest.ProxmoxVE.Admin.VmUnlock;

public class Module : PveAdminModuleBase, IForceLoadModule
{
    public Module()
    {
        Authors = "Corsinvest Srl";
        Company = "Corsinvest Srl";
        Keywords = "Vm,Unlock";
        Description = "Vm Unlock";
        InfoText = "Unlock VM/CT that have remained in the \"locked\" status";
        SetCategory(AdminModuleCategory.Utilities);

        Link = new ModuleLink(this, Description)
        {
            Icon = Icons.Material.Filled.Lock,
            Render = typeof(RenderIndex)
        };

        Widgets =
        [
            new ModuleWidget(this,"Status")
            {
                GroupName = Category,
                Render= typeof(Widget),
                Class = "mud-grid-item mud-grid-item-xs-12 mud-grid-item-sm-6 mud-grid-item-md-4 mud-grid-item-lg-4"
            }
        ];

        Roles = [new("", "", Permissions.DataGrid.Data.Permissions.Union([Permissions.DataGrid.Unlock]))];

        UrlHelp += "#chapter_module_vm_unlock";
    }

    public class Permissions
    {
        public class DataGrid
        {
            public static PermissionsRead Data { get; } = new($"{typeof(Module).FullName}.{nameof(DataGrid)}.{nameof(Data)}");
            public static Permission Unlock { get; } = new($"{Data.Prefix}.{nameof(Unlock)}", "Unlock", Icons.Material.Filled.LockOpen, UIColor.Error);
        }
    }
}