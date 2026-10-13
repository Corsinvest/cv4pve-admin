/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Components.Widgets;
using Corsinvest.ProxmoxVE.Admin.Core.Helpers;

namespace Corsinvest.ProxmoxVE.Admin.Module.ReplicationAnalytics.Components.Widgets.Check;

public partial class Render(IDbContextFactory<ModuleDbContext> dbContextFactory,
                            IAdminService adminService,
                            ISettingsService settingsService) : WidgetThumbDetailsBase<CheckSettings>(adminService, settingsService)
{
    protected override async Task RefreshDataAsyncInt()
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();

        // The clusters where the scan is scheduled, and those where a scan was started by hand
        var requested = GetRequestedClusterNames();
        var withData = await db.JobResults
                               .Where(a => requested.Contains(a.ClusterName))
                               .Select(a => a.ClusterName)
                               .Distinct()
                               .ToListAsync();

        var clusterNames = GetClusterNames<Module, Settings>().Union(withData).ToList();
        if (clusterNames.Count == 0)
        {
            Status = WidgetState.NotConfigured;
            Count = 0;
            Items = [];
            Message = L["Module not configured!"];
            return;
        }

        var raw = await db.JobResults
                          .Where(a => clusterNames.Contains(a.ClusterName))
                          .Where(a => !a.Status && a.Start > DateTime.UtcNow.AddDays(-Settings.Day))
                          .GroupBy(a => new { a.VmId, a.ClusterName })
                          .Select(a => new { a.Key.VmId, a.Key.ClusterName, Count = a.Count() })
                          .ToListAsync();

        Items = [.. raw.Select(r => new Data(
            string.IsNullOrEmpty(r.VmId) ? "—" : $"VM {r.VmId}",
            r.Count,
            long.TryParse(r.VmId, out var id)
                ? UrlHelper.Resources.VmUrl(id, r.ClusterName)
                : $"{UrlHelper.ModuleUrl("replication-analytics", r.ClusterName)}/replications"))];

        Count = Items.Sum(a => a.Count);
        Status = Count > 0 ? WidgetState.Issues : WidgetState.Ok;
        Message = L["Last {0} days", Settings.Day];
    }
}
