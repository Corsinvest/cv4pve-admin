/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.DependencyInjection;
using Corsinvest.ProxmoxVE.Admin.Core.Services.DiskInfo;
using Corsinvest.ProxmoxVE.Api;

namespace Corsinvest.ProxmoxVE.Admin.Core.Services;

public interface IPveClientService : IScopedDependency
{
    Task<PveClient?> GetClientAsync(ClusterOptions clusterOptions);
    Task<PveClient?> GetClientAsync(ClusterOptions clusterOptions, ILogger logger);
    Task<PveClient?> GetClientAsync(string clusterName);
    Task<PveClient> GetClientCurrentClusterAsync();
    ClusterOptions? GetClusterOptions(string clusterName);
    Task<ClusterOptions?> GetCurrentClusterOptionsAsync();
    IEnumerable<ClusterOptions> GetClusters();
    Task SetCurrentClusterNameAsync(string clusterName);
    Task<string> GetCurrentClusterNameAsync();
    Task<bool> ExistsCurrentClusterNameAsync();
    Task<bool> ClusterIsValidAsync(string clusterName);
    Task<int> PopulateInfoNodesAsync(ClusterOptions clusterOptions);
    Task<bool> CheckIsValidVersionAsync(PveClient client);
    Task<Api.Shared.Models.Cluster.ClusterStatus?> GetClusterStatusAsync(PveClient client);
    string GetUrl(ClusterOptions clusterOptions);
    Task<IEnumerable<DiskInfoBase>> GetDisksInfo(PveClient client, ClusterOptions clusterOptions);
}