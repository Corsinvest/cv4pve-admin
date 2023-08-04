/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.Modularity;
using Corsinvest.ProxmoxVE.Admin.Core.Modularity;

namespace Corsinvest.ProxmoxVE.Admin.Core.Support.Buy;

public class Module : PveAdminModuleBase, IForceLoadModule
{
    public Module()
    {
        Authors = "Corsinvest Srl";
        Company = "Corsinvest Srl";
        Keywords = "Subscription,Buy Subscription";
        SetCategory(AdminModuleCategory.Support);
        Icon = Icons.Material.Filled.Shield;
        Type = ModuleType.Application;
        Description = "Buy Subscription";
        Slug = "BuySubscription";

        Link = new ModuleLink(this, Description, "https://shop.corsinvest.it", true)
        {
            Icon = Icons.Material.Filled.ShoppingCart,
            Order = 2
        };
    }
}
