/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.VzDumpTrend;

internal class Job(IServiceScopeFactory scopeFactory)
{
    private static bool ModuleEnabled(IServiceScope scope) => scope.GetModule<Module>()!.Enabled;

    public async Task Scan(string clusterName)
    {
        using var scope = scopeFactory.CreateScope();
        if (ModuleEnabled(scope)) { await Helper.Scan(scope, clusterName); }
    }
}