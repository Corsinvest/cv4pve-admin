/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Ardalis.Specification;
using Corsinvest.ProxmoxVE.Admin.AutoSnap.Models;
using Corsinvest.ProxmoxVE.Admin.Core.Repository;

namespace Corsinvest.ProxmoxVE.Admin.AutoSnap.Repository;

internal class AutoSnapJobSpec : ClusterByNameSpec<AutoSnapJob>
{
    public AutoSnapJobSpec(string clusterName) : base(clusterName)
        => Query.Include(a => a.Hooks).AsSplitQuery();

    public AutoSnapJobSpec(int id) : base(string.Empty)
        => Query.Where(a => a.Id == id)
                .Include(a => a.Histories)
                .Include(a => a.Hooks)
                .AsSplitQuery();

    public AutoSnapJobSpec Enabled()
    {
        Query.Where(a => a.Enabled);
        return this;
    }
}
