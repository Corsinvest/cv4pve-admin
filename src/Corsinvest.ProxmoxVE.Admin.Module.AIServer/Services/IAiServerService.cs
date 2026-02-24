/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Module.AIServer.Services;

public interface IAiServerService
{
    string SerializeTable<T>(IEnumerable<T> items);
    Task<bool> CanExecuteToolAsync(string clusterName, Permission permission);
    (ClusterClient? Client, string? ErrorJson) GetClusterClient(string clusterName);
    Task<IEnumerable<ClusterResource>> HasAsync(string clusterName, IEnumerable<ClusterResource> items);
}
