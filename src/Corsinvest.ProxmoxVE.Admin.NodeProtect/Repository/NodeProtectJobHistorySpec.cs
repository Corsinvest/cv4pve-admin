/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Ardalis.Specification;
using Corsinvest.ProxmoxVE.Admin.Core.Repository;

namespace Corsinvest.ProxmoxVE.Admin.NodeProtect.Repository;

internal class NodeProtectJobHistorySpec : ClusterByNameSpec<NodeProtectJobHistory>
{
    public NodeProtectJobHistorySpec(string clusterName, DateTime start, int keep) : base(clusterName)
        => Query.Where(a => a.Start < start)
                .OrderByDescending(a => a.End)
                .Skip(keep);
}