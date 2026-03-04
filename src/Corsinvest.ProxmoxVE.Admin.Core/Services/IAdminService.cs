/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Services;

public interface IAdminService : IEnumerable<ClusterClient>
{
    ClusterClient this[string clusterName] { get; }
    bool Exists(string clusterName);
    Task<FluentResults.Result<string>> PopulateInfoAsync(ClusterSettings clusterSettings);
    Task<FluentResults.Result<string>> TestSshAsync(ClusterSettings clusterSettings);
    ClusterClient CreateClusterClient(ClusterSettings clusterSettings);
}
