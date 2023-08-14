/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.Modularity;
using Corsinvest.ProxmoxVE.Admin.Core.Modularity;

namespace Corsinvest.ProxmoxVE.Admin.Core.Support.RequestSupport;

public class Module : PveAdminModuleBase, IForceLoadModule
{
    public Module()
    {
        Authors = "Corsinvest Srl";
        Company = "Corsinvest Srl";
        Keywords = "Subscription,Request Support";
        SetCategory(AdminModuleCategory.Support);
        Icon = Icons.Material.Filled.Shield;
        Type = ModuleType.Application;
        Description = "Request Support";
        Slug = "RequestSupport";

        Link = new ModuleLink(this, Description)
        {
            Icon = Icons.Material.Filled.SupportAgent,
            Render = typeof(RenderSupport),
            Order = 3
        };
    }
}