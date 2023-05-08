/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Ardalis.Specification;

namespace Corsinvest.ProxmoxVE.Admin.Core.Repository;

public class ClusterByNameSpec<T> : Specification<T> where T : IClusterName
{
    public ClusterByNameSpec(string clusterName)
        => Query.Where(c => c.ClusterName == clusterName, !string.IsNullOrEmpty(clusterName));
}