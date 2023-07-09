/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.Modularity;
using Corsinvest.AppHero.Core.Security.Auth.Permissions;
using Corsinvest.ProxmoxVE.Admin.Core.Modularity;

namespace Corsinvest.ProxmoxVE.Admin.QemuMonitor;

public class Module : PveAdminModuleBase, IForceLoadModule
{
    public Module()
    {
        Authors = "Corsinvest Srl";
        Company = "Corsinvest Srl";
        Keywords = "QEMU,Monitor,Info,Performance";
        Description = "Qemu Monitor";
        InfoText = "Proxmox VE does not allow the operating system to view IOPS for VM. With this it is easy to identify the virtual machines and solve the problem";
        SetCategory(AdminModuleCategory.Health);

        Link = new ModuleLink(this, Description)
        {
            Icon = Icons.Material.Filled.DisplaySettings,
            Render = typeof(RenderIndex)
        };

        Roles = new Role[]
        {
            new("","", Permissions.DataGrid.Data.Permissions)
        };

        UrlHelp += "#chapter_module_qemu_monitor";
    }

    public class Permissions
    {
        public class DataGrid
        {
            public static PermissionsRead Data { get; } = new($"{typeof(Module).FullName}.{nameof(DataGrid)}.{nameof(Data)}");
        }
    }
}