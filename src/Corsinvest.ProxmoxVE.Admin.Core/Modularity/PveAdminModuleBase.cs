/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.Modularity;

namespace Corsinvest.ProxmoxVE.Admin.Core.Modularity;

public abstract class PveAdminModuleBase : ModuleBase
{
    protected void SetCategory(AdminModuleCategory category)
    {
        if (PveAdminHelper.ModuleCategories.TryGetValue(category, out var value)) { Category = value.Name; }
    }

    public IEnumerable<string> PvePermissionRequired { get; protected set; } = [];
}
