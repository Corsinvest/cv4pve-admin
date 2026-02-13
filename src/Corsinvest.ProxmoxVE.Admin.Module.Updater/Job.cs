/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Module.Updater.Helpers;

namespace Corsinvest.ProxmoxVE.Admin.Module.Updater;

internal class Job(IServiceScopeFactory scopeFactory)
{
    public async Task ScanAsync(string clusterName)
    {
        using var scope = scopeFactory.CreateScope();
        await ActionHelper.ScanAsync(scope, clusterName, false);
    }

    public async Task ScanFromResultAsync(string clusterName)
    {
        using var scope = scopeFactory.CreateScope();
        await ActionHelper.ScanAsync(scope, clusterName, false);
    }
}
