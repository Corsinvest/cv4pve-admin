/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Ardalis.Specification;

namespace Corsinvest.ProxmoxVE.Admin.Diagnostic.Repository;

internal class ExecutionSpec : ClusterByNameSpec<Execution>
{
    public ExecutionSpec(string clusterName) : base(clusterName)
    {
        //Query.OrderByDescending(a => a.Date);
    }

    public ExecutionSpec(string clusterName, int keep) : this(clusterName)
        => Query.OrderByDescending(a => a.Date)
                .Skip(keep);

    public ExecutionSpec Last()
    {
        Query.OrderBy(a => a.Date);
        return this;
    }

    public ExecutionSpec Include()
    {
        Query.Include(a => a.Data);
        return this;
    }

    public ExecutionSpec ByKey(int id)
    {
        Query.Where(a => a.Id == id);
        return this;
    }
}
