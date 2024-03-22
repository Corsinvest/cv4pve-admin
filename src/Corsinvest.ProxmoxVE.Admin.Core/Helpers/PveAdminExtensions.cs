/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Api;
using Corsinvest.ProxmoxVE.Api.Shared.Utils;

namespace Corsinvest.ProxmoxVE.Admin.Core.Helpers;

public static class PveAdminExtensions
{
    public static async Task<(string Type, string Name)> GetClusterInfo(this PveClient client)
    {
        var status = await client.Cluster.Status.Get();
        var clusterName = status.FirstOrDefault(a => a.Type == PveConstants.KeyApiCluster)?.Name;
        var type = string.IsNullOrEmpty(clusterName)
                        ? "NODE"
                        : "CLUSTER";

        var name = string.IsNullOrEmpty(clusterName)
                        ? status.FirstOrDefault()!.Name
                        : clusterName;
        return (type, name);
    }
}
