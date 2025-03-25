/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.Extensions;

namespace Corsinvest.ProxmoxVE.Admin.NodeProtect;

internal class Job(IServiceScopeFactory scopeFactory)
{
    private static bool ModuleEnabled(IServiceScope scope) => scope.GetModule<Module>()!.Enabled;

    public async Task Protect(string clusterName)
    {
        using var scope = scopeFactory.CreateScope();
        if (ModuleEnabled(scope)) { await Helper.Protect(scope, clusterName); }
    }
}