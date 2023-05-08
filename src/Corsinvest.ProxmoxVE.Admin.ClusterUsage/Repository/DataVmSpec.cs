/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Ardalis.Specification;
using Corsinvest.ProxmoxVE.Admin.Core.Repository;

namespace Corsinvest.ProxmoxVE.Admin.ClusterUsage.Repository;

internal class DataVmSpec : ClusterByNameSpec<DataVm>
{
    public DataVmSpec(string clusterName) : base(clusterName) { }

    public DataVmSpec(string clusterName, long vmId, DateTime date) : base(clusterName)
        => Query.Where(a => a.VmId == vmId && a.Date == date);


    //public DataVmSpec(string clusterName, bool status, DateTime date) : this(clusterName)
    //    => Query.Where(a => a.Status == status && a.Start >= date);

    //public DataVmSpec(string clusterName, DateTime start, DateTime end, bool status) : this(clusterName)
    //    => Query.Where(a => a.Status == status && a.Start > start && a.Start < end);

    //public DataVmSpec StorageExists()
    //{
    //    Query.Where(a => !string.IsNullOrWhiteSpace(a.Task.Storage));
    //    return this;
    //}

    //public DataVmSpec InDate(bool condition, DateTime? start, DateTime? end)
    //{
    //    Query.Where(a => a.Start > start && a.Start < end, condition);
    //    return this;
    //}

    //public DataVmSpec InStorage(bool condition, string storage)
    //{
    //    Query.Where(a => a.Task.Storage == storage, condition);
    //    return this;
    //}

    //public DataVmSpec InVm(bool condition, string? vmId)
    //{
    //    Query.Where(a => a.VmId == vmId, condition);
    //    return this;
    //}

}
