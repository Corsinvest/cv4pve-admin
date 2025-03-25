/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Ardalis.Specification;

namespace Corsinvest.ProxmoxVE.Admin.VzDumpTrend.Repository;

internal class VzDumpTaskSpec(string clusterName) : ClusterByNameSpec<VzDumpTask>(clusterName)
{
    public VzDumpTaskSpec(string clusterName, string taskId) : this(clusterName)
        => Query.Where(a => a.TaskId == taskId);

    public VzDumpTaskSpec Over(DateTime date)
    {
        Query.Where(a => a.Start < date);
        return this;
    }
}