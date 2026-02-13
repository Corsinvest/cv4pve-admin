/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Services;

public interface IAdminService : IEnumerable<ClusterClient>
{
    ClusterClient this[string clusterName] { get; }
    bool Exists(string clusterName);
    Task<string> GetCurrentClusterNameAsync();
    Task SetCurrentClusterNameAsync(string clusterName);
    Task<FluentResults.Result<string>> PopulateInfoAsync(ClusterSettings clusterSettings);
}
