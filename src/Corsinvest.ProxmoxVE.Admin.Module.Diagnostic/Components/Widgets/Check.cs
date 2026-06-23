/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Components.Widgets;
using Corsinvest.ProxmoxVE.Admin.Core.Helpers;
using Corsinvest.ProxmoxVE.Admin.Module.Diagnostic.Services;
using Corsinvest.ProxmoxVE.Diagnostic.Api;

namespace Corsinvest.ProxmoxVE.Admin.Module.Diagnostic.Components.Widgets;

public partial class Check(IDbContextFactory<ModuleDbContext> dbContextFactory,
                           IAdminService adminService,
                           ISettingsService settingsService,
                           IDiagnosticService diagnosticService) : WidgetThumbDetailsBase<object>(adminService, settingsService)
{
    protected override async Task RefreshDataAsyncInt()
    {
        var clusterNames = GetClusterNames<Module, Settings>();
        if (!clusterNames.Any())
        {
            Status = WidgetState.NotConfigured;
            Count = 0;
            Items = [];
            Message = L["Module not configured!"];
            return;
        }

        await using var db = await dbContextFactory.CreateDbContextAsync();

        var allResults = new List<Data>();
        DateTime? latestScan = null;

        foreach (var clusterName in clusterNames)
        {
            var lastJob = await db.JobResults
                                  .Where(a => a.ClusterName == clusterName)
                                  .OrderByDescending(a => a.Start)
                                  .Select(a => new { a.Id, a.Start })
                                  .FirstOrDefaultAsync();
            if (lastJob is null || lastJob.Id <= 0) { continue; }

            if (latestScan is null || lastJob.Start > latestScan)
            {
                latestScan = lastJob.Start;
            }

            var scanUrl = $"{UrlHelper.ModuleUrl("diagnostic", clusterName)}/scans?Id={lastJob.Id}";

            var clusterResults = await db.JobDetails
                                         .Where(a => a.JobResult.Id == lastJob.Id && a.Gravity == DiagnosticResultGravity.Critical)
                                         .OrderBy(a => a.Context)
                                         .ThenBy(a => a.IdResource)
                                         .GroupBy(a => new { a.IdResource, a.Context })
                                         .Select(a => new { a.Key.Context, a.Key.IdResource, Count = a.Count() })
                                         .ToListAsync();

            allResults.AddRange(clusterResults.Select(r => new Data(
                diagnosticService.GetResourceLabel(r.IdResource, r.Context, clusterName),
                r.Count,
                scanUrl)));
        }

        Items = [.. allResults];
        Count = Items.Sum(a => a.Count);
        Status = Count > 0 ? WidgetState.Issues : WidgetState.Ok;
        Message = latestScan.HasValue
                    ? L["Last scan: {0}", latestScan.Value.ToLocalTime().ToString("g")]
                    : L["No scan yet"];
    }
}
