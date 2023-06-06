/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Ardalis.Specification;
using Corsinvest.ProxmoxVE.Admin.AutoSnap.Models;

namespace Corsinvest.ProxmoxVE.Admin.Core.Repository;

internal class AutoSnapJobHookSpec : Specification<AutoSnapJobHook>
{
   // public AutoSnapJobHookSpec(string clusterName) : base(clusterName) { }
    public AutoSnapJobHookSpec(int jobId) => Query.Where(a => a.Job.Id == jobId);
    //public AutoSnapJobHookSpec(int id) : base(string.Empty) => Query.Where(a => a.Id == id);

    //public AutoSnapJobHookSpec Include()
    //{
    //    Query.Include(a => a.Histories)
    //          .Include(a => a.Hooks)
    //          .AsSplitQuery();

    //    return this;
    //}

    //public AutoSnapJobHookSpec Enabled()
    //{
    //    Query.Where(a => a.Enabled);

    //    return this;
    //}
}