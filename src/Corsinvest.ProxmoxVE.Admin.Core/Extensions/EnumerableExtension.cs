/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Repository;

namespace Corsinvest.ProxmoxVE.Admin.Core.Extensions;

public static class EnumerableExtension
{
    public static ulong Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, ulong> summer)
    {
        ulong total = 0;
        foreach (var item in source) { total += summer(item); }
        return total;
    }

    public static T? IsCluster<T>(this IEnumerable<T> query, string clusterName) where T : IClusterName
        => query.FirstOrDefault(a => a.ClusterName == clusterName);
}
