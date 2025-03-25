/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Diagnostic;

internal class Job(IServiceScopeFactory scopeFactory)
{
    private static bool ModuleEnabled(IServiceScope scope) => scope.GetModule<Module>()!.Enabled;

    public async Task Create(string clusterName)
    {
        using var scope = scopeFactory.CreateScope();
        if (ModuleEnabled(scope)) { await Helper.Create(scope, clusterName); }
    }

    public async Task Delete(IEnumerable<int> ids)
    {
        using var scope = scopeFactory.CreateScope();
        await Helper.Delete(scope, ids);
    }

    public async Task Rescan(string clusterName)
    {
        using var scope = scopeFactory.CreateScope();
        await Helper.Rescan(scope, clusterName);
    }

    public static void ScheduleRescan(IJobService JobService, string clusterName)
        => JobService.Schedule<Job>(a => a.Rescan(clusterName), TimeSpan.FromSeconds(10));
}