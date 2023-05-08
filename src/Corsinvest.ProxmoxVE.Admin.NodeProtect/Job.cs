/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.Extensions;

namespace Corsinvest.ProxmoxVE.Admin.NodeProtect;

internal class Job
{
    private readonly IServiceScopeFactory _scopeFactory;
    public Job(IServiceScopeFactory scopeFactory) => _scopeFactory = scopeFactory;
    private static bool ModuleEnabled(IServiceScope scope) => scope.GetModule<Module>()!.Enabled;

    public async Task Protect(string clusterName)
    {
        using var scope = _scopeFactory.CreateScope();
        if (ModuleEnabled(scope)) { await Helper.Protect(scope, clusterName); }
    }
}