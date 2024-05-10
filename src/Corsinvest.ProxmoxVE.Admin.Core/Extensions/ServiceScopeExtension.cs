/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Repository;
using Corsinvest.ProxmoxVE.Admin.Core.UI.Options;
using Corsinvest.ProxmoxVE.Api;
using Microsoft.Extensions.DependencyInjection;

namespace Corsinvest.ProxmoxVE.Admin.Core.Extensions;

public static class ServiceScopeExtension
{
    public static async Task<PveClient> GetPveClientAsync(this IServiceScope scope, string clusterName)
        => (await scope.GetPveClientService().GetClientAsync(clusterName))!;

    public static IPveClientService GetPveClientService(this IServiceScope scope)
        => scope.ServiceProvider.GetRequiredService<IPveClientService>();

    public static TModuleClusterOptions GetModuleClusterOptions<TOptions, TModuleClusterOptions>(this IServiceScope scope, string clusterName)
        where TOptions : PveModuleClustersOptions<TModuleClusterOptions>
        where TModuleClusterOptions : IClusterName
        => scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<TOptions>>().Value.Get(clusterName);
}