/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.ClusterUsage;

internal class Job
{
    private readonly IServiceScopeFactory _scopeFactory;
    public Job(IServiceScopeFactory scopeFactory) => _scopeFactory = scopeFactory;
    private static bool ModuleEnabled(IServiceScope scope) => scope.GetModule<Module>()!.Enabled;

    public async Task Scan(string clusterName)
    {
        using var scope = _scopeFactory.CreateScope();
        if (ModuleEnabled(scope)) { await Helper.Scan(scope, clusterName); }
    }
}