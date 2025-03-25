/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Ardalis.Specification;

namespace Corsinvest.ProxmoxVE.Admin.ReplicationTrend.Repository;

internal class ReplicationResultSpec(string clusterName) : ClusterByNameSpec<ReplicationResult>(clusterName)
{
    public ReplicationResultSpec(string clusterName, DateTime start, DateTime end, bool status) : this(clusterName)
        => Query.Where(a => a.Status == status && a.Start > start && a.Start < end);

    public ReplicationResultSpec Exists(string jobId, DateTime lastSync)
    {
        Query.Where(a => a.JobId == jobId && a.LastSync == lastSync);
        return this;
    }

    public ReplicationResultSpec OrderDescStart()
    {
        Query.OrderByDescending(a => a.Start);
        return this;
    }

    public ReplicationResultSpec Over(DateTime date)
    {
        Query.Where(a => a.Start < date);
        return this;
    }

    public ReplicationResultSpec(string clusterName, bool status, DateTime date) : this(clusterName)
        => Query.Where(a => a.Status == status && a.Start >= date);

    public ReplicationResultSpec InDate(bool condition, DateTime? start, DateTime? end)
    {
        Query.Where(a => a.Start > start && a.Start < end, condition);
        return this;
    }

    public ReplicationResultSpec InVm(bool condition, string? vmId)
    {
        Query.Where(a => a.VmId == vmId, condition);
        return this;
    }
}