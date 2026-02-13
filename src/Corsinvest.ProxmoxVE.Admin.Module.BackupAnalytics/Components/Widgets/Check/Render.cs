/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Components.Widgets;

namespace Corsinvest.ProxmoxVE.Admin.Module.BackupAnalytics.Components.Widgets.Check;

public partial class Render(IDbContextFactory<ModuleDbContext> dbContextFactory,
                            IAdminService adminService,
                            ISettingsService settingsService) : WidgetThumbDetailsBase<CheckSettings>(adminService, settingsService)
{
    protected override async Task RefreshDataAsyncInt()
    {
        var clusterNames = GetClusterNames<Module, Settings>();
        if (clusterNames.Any())
        {
            await using var db = await dbContextFactory.CreateDbContextAsync();

            Items = [.. await db.TaskResults
                                .Where(a => clusterNames.Contains(a.ClusterName))
                                .SelectMany(a => a.Jobs)
                                .Where(a => !a.Status && a.Start > DateTime.UtcNow.AddDays(-Settings.Day))
                                .GroupBy(a => a.VmId)
                                .Select(a => new Data(a.Key, a.Count()))
                                .ToListAsync()];

            Message = L["Last {0} days", Settings.Day];
        }
        else
        {
            ShowIcon = false;
            Items = [];
            Message = L["Module not configured!"];
        }
    }
}
