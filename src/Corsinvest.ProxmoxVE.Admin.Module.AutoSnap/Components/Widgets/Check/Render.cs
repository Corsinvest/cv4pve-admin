/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Components.Widgets;

namespace Corsinvest.ProxmoxVE.Admin.Module.AutoSnap.Components.Widgets.Check;

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

        Count = await db.Jobs
                        .Where(a => clusterNames.Contains(a.ClusterName))
                        .SelectMany(a => a.Results)
                        .Where(a => !a.Status && a.Start > DateTime.UtcNow.AddDays(-Settings.Day))
                        .CountAsync();

        Status = Count > 0 ? WidgetState.Issues : WidgetState.Ok;
        Items = [];
        Message = L["Last {0} days", Settings.Day];
    }
}
