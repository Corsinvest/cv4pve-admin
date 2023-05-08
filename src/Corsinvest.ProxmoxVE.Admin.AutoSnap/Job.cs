/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.Extensions;
using Corsinvest.ProxmoxVE.Admin.AutoSnap.Models;

namespace Corsinvest.ProxmoxVE.Admin.AutoSnap;

internal class Job
{
    private readonly IServiceScopeFactory _scopeFactory;
    public Job(IServiceScopeFactory scopeFactory) => _scopeFactory = scopeFactory;
    private static bool ModuleEnabled(IServiceScope scope) => scope.GetModule<Module>()!.Enabled;

    public async Task Create(int id)
    {
        using var scope = _scopeFactory.CreateScope();
        if (ModuleEnabled(scope)) { await Helper.Create(scope, id); }
    }

    public async Task Clean(int id)
    {
        using var scope = _scopeFactory.CreateScope();
        await Helper.Clean(scope, id);
    }

    public async Task Delete(IEnumerable<int> ids)
    {
        using var scope = _scopeFactory.CreateScope();
        await Helper.Delete(scope, ids);
    }

    public async Task Delete(IEnumerable<AutoSnapInfo> snapshots, string clusterName)
    {
        using var scope = _scopeFactory.CreateScope();
        await Helper.Delete(scope, snapshots, clusterName);
    }
}
