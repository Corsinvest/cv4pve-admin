using Corsinvest.ProxmoxVE.Admin.Core.Components.Widgets;
using Corsinvest.ProxmoxVE.Diagnostic.Api;

namespace Corsinvest.ProxmoxVE.Admin.Module.Diagnostic.Components.Widgets;

public partial class Check(IDbContextFactory<ModuleDbContext> dbContextFactory,
                           IAdminService adminService,
                           ISettingsService settingsService) : WidgetThumbDetailsBase<object>(adminService, settingsService)
{
    protected override async Task RefreshDataAsyncInt()
    {
        var clusterNames = GetClusterNames<Module, Settings>();
        if (clusterNames.Any())
        {
            await using var db = await dbContextFactory.CreateDbContextAsync();

            var allResults = new List<Data>();

            foreach (var clusterName in clusterNames)
            {
                var id = await db.JobResults
                                 .Where(a => a.ClusterName == clusterName)
                                 .OrderByDescending(a => a.Start)
                                 .Select(a => a.Id)
                                 .FirstOrDefaultAsync();
                if (id > 0)
                {
                    var clusterResults = await db.JobDetails
                                                 .Where(a => a.JobResult.Id == id && a.Gravity == DiagnosticResultGravity.Critical) //a.Gravity == DiagnosticResultGravity.Warning
                                                 .OrderBy(a => a.Context)
                                                 .ThenBy(a => a.IdResource)
                                                 .GroupBy(a => new { a.IdResource, a.Context })
                                                 .Select(a => new Data($"{clusterName}/{a.Key.Context}/{a.Key.IdResource}", a.Count()))
                                                 .ToListAsync();

                    allResults.AddRange(clusterResults);
                }
            }

            Items = [.. allResults];

            Message = L["Latest scan"];
        }
        else
        {
            ShowIcon = false;
            Items = [];
            Message = L["Module not configured!"];
        }
    }
}
