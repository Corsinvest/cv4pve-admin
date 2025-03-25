/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.Extensions;
using Corsinvest.ProxmoxVE.Admin.AutoSnap.Models;

namespace Corsinvest.ProxmoxVE.Admin.AutoSnap;

internal class Job(IServiceScopeFactory scopeFactory)
{
    private static bool ModuleEnabled(IServiceScope scope) => scope.GetModule<Module>()!.Enabled;

    public async Task CreateAsync(int id)
    {
        using var scope = scopeFactory.CreateScope();
        if (ModuleEnabled(scope)) { await Helper.CreateAsync(scope, id); }
    }

    public async Task PurgeAsync(int id)
    {
        using var scope = scopeFactory.CreateScope();
        await Helper.PurgeAsync(scope, id);
    }

    public async Task DeleteAsync(IEnumerable<int> ids)
    {
        using var scope = scopeFactory.CreateScope();
        await Helper.DeleteAsync(scope, ids);
    }

    public async Task DeleteAsync(IEnumerable<AutoSnapInfo> snapshots, string clusterName)
    {
        using var scope = scopeFactory.CreateScope();
        await Helper.DeleteAsync(scope, snapshots, clusterName);
    }
}
