/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Ardalis.Specification;
using Corsinvest.ProxmoxVE.Admin.AutoSnap.Models;

namespace Corsinvest.ProxmoxVE.Admin.Core.Repository;

internal class AutoSnapJobSpec : ClusterByNameSpec<AutoSnapJob>
{
    public AutoSnapJobSpec(string clusterName) : base(clusterName) { }
    public AutoSnapJobSpec(int id) : base(string.Empty) => Query.Where(a => a.Id == id);

    public AutoSnapJobSpec Include()
    {
        Query.Include(a => a.Histories)
              .Include(a => a.Hooks)
              .AsSplitQuery();

        return this;
    }

    public AutoSnapJobSpec Enabled()
    {
        Query.Where(a => a.Enabled);

        return this;
    }
}