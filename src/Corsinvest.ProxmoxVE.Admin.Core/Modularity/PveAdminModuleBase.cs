/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.Modularity;

namespace Corsinvest.ProxmoxVE.Admin.Core.Modularity;

public abstract class PveAdminModuleBase : ModuleBase
{
    protected void SetCategory(ModuleCategory category) => Category = PveAdminHelper.GetCategoryName(category);

    public IEnumerable<string> PvePermissionRequired { get; protected set; } = Enumerable.Empty<string>();
}
