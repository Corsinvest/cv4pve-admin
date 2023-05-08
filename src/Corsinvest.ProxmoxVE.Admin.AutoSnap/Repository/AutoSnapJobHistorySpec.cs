/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Ardalis.Specification;
using Corsinvest.ProxmoxVE.Admin.AutoSnap.Models;

namespace Corsinvest.ProxmoxVE.Admin.Core.Repository;

internal class AutoSnapJobHistorySpec : ClusterByNameSpec<AutoSnapJobHistory>
{
    public AutoSnapJobHistorySpec(string clusterName, int jobId, bool showOnlyError) : base(clusterName)
    {
        Query.Where(a => a.Job.Id == jobId, jobId > 0)
             .Where(a => !a.Status, showOnlyError);
    }

    public AutoSnapJobHistorySpec(string clusterName) : base(clusterName)
    {
        Query.OrderByDescending(a => a.Start.Date);
    }

    public AutoSnapJobHistorySpec InError(int days)
    {
        var start = DateTime.Now.Date.AddDays(-days).Date;
        Query.Where(a => !a.Status && a.Start >= start);
        return this;
    }

}
