/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.ClusterUsage;

internal class Job(IServiceScopeFactory scopeFactory)
{
    private static bool ModuleEnabled(IServiceScope scope) => scope.GetModule<Module>()!.Enabled;

    public async Task ScanAsync(string clusterName)
    {
        using var scope = scopeFactory.CreateScope();
        if (ModuleEnabled(scope)) { await Helper.ScanAsync(scope, clusterName); }
    }
}