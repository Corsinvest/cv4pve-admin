/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Module.AutoSnap;

internal class Job(IServiceScopeFactory scopeFactory)
{
    public async Task SnapAsync(int id)
    {
        using var scope = scopeFactory.CreateScope();
        await ActionHelper.SnapAsync(scope, id);
    }

    public async Task PurgeAsync(int id)
    {
        using var scope = scopeFactory.CreateScope();
        await ActionHelper.PurgeAsync(scope, id);
    }

    public async Task DeleteAsync(int id)
    {
        using var scope = scopeFactory.CreateScope();
        await ActionHelper.DeleteAsync(scope, id);
    }

    public async Task DeleteAsync(IEnumerable<AutoSnapInfo> snapshots, string clusterName)
    {
        using var scope = scopeFactory.CreateScope();
        await ActionHelper.DeleteAsync(scope, snapshots, clusterName);
    }
}
