/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.DependencyInjection;
using Corsinvest.ProxmoxVE.Api;

namespace Corsinvest.ProxmoxVE.Admin.Core.Services;

public interface IPveClientService : IScopedDependency
{
    Task<PveClient?> GetClient(ClusterOptions clusterOptions);
    Task<PveClient?> GetClient(string clusterName);
    Task<PveClient> GetClientCurrentCluster();
    ClusterOptions? GetClusterOptions(string clusterName);
    Task<ClusterOptions?> GetCurrentClusterOptions();
    IDictionary<string, string> GetClustersNames();
    Task<IEnumerable<string>> GetClustersNames(bool onlyFirst);
    Task SetCurrentClusterName(string clusterName);
    Task<string> GetCurrentClusterName();
    Task<bool> IsValidCurrentClusterName();
}