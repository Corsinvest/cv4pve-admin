/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Repository;

namespace Corsinvest.ProxmoxVE.Admin.Core.Extensions;

public static class QueryableExtension
{
    public static T? IsCluster<T>(this IQueryable<T> query, string clusterName) where T : IClusterName
        => query.FirstOrDefault(a => a.ClusterName == clusterName);
}