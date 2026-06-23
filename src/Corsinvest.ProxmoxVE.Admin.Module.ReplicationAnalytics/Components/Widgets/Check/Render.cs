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
