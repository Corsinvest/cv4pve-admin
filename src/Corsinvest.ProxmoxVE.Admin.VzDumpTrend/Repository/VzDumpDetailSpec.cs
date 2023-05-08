/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Ardalis.Specification;

namespace Corsinvest.ProxmoxVE.Admin.VzDumpTrend.Repository;

internal class VzDumpDetailSpec : Specification<VzDumpDetail>
{

    public VzDumpDetailSpec(string clusterName)
        => Query.Include(a => a.Task)
                .Where(a => a.Task.ClusterName == clusterName);

    public VzDumpDetailSpec OrderDescStart()
    {
        Query.OrderByDescending(a => a.Start);
        return this;
    }

    public VzDumpDetailSpec(string clusterName, bool status, DateTime date) : this(clusterName)
        => Query.Where(a => a.Status == status && a.Start >= date);

    public VzDumpDetailSpec(string clusterName, DateTime start, DateTime end, bool status) : this(clusterName)
        => Query.Where(a => a.Status == status && a.Start > start && a.Start < end);

    public VzDumpDetailSpec StorageExists()
    {
        Query.Where(a => !string.IsNullOrWhiteSpace(a.Task.Storage));
        return this;
    }

    public VzDumpDetailSpec InDate(bool condition, DateTime? start, DateTime? end)
    {
        Query.Where(a => a.Start > start && a.Start < end, condition);
        return this;
    }

    public VzDumpDetailSpec InStorage(bool condition, string storage)
    {
        Query.Where(a => a.Task.Storage == storage, condition);
        return this;
    }

    public VzDumpDetailSpec InVm(bool condition, string? vmId)
    {
        Query.Where(a => a.VmId == vmId, condition);
        return this;
    }
}
