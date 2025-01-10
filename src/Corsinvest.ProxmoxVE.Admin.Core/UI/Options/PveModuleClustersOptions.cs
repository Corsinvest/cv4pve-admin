/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Repository;

namespace Corsinvest.ProxmoxVE.Admin.Core.UI.Options;

public abstract class PveModuleClustersOptions<T> where T : IClusterName
{
    public List<T> Clusters { get; set; } = [];

    public T Get(string clusterName)
    {
        var ret = Clusters.IsCluster(clusterName);
        if (ret == null)
        {
            ret = Activator.CreateInstance<T>();
            ret.ClusterName = clusterName;
            Clusters.Add(ret);
        }

        return ret;
    }
}
