/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Module.SystemReport.Helpers;

namespace Corsinvest.ProxmoxVE.Admin.Module.SystemReport;

internal class Job(IServiceScopeFactory scopeFactory)
{
    public async Task ScanAsync(int id)
    {
        using var scope = scopeFactory.CreateScope();
        await ActionHelper.ScanAsync(scope, id);
    }
}
