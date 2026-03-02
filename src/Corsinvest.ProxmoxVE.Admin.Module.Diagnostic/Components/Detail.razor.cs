/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Helpers;
using Corsinvest.ProxmoxVE.Admin.Module.Diagnostic.Services;
using Corsinvest.ProxmoxVE.Diagnostic.Api;

namespace Corsinvest.ProxmoxVE.Admin.Module.Diagnostic.Components;

public partial class Detail(IDbContextFactory<ModuleDbContext> dbContextFactory,
                            NotificationService notificationService,
                            IAdminService adminService,
                            IDiagnosticService diagnosticService)
{
    [CascadingParameter(Name = nameof(ClusterName))] public string ClusterName { get; set; } = default!;
    [Parameter] public int ResultId { get; set; } = default!;

    private RadzenDataGrid<JobDetail> DataGridRef { get; set; } = default!;
    private string _baseAddress { get; set; } = default!;
    private IEnumerable<JobDetail> Items { get; set; } = [];

    private string GetHelpUrl(JobDetail item) => diagnosticService.GetHelpUrl(item);
    private string GetPveUrl(JobDetail item)
    {
        var data = item.IdResource.Split("/");
        return item.Context switch
        {
            DiagnosticResultContext.Node => UrlHelper.Resources.NodeUrl(data[1]),
            DiagnosticResultContext.Cluster or DiagnosticResultContext.Storage => "#",
            DiagnosticResultContext.Qemu or DiagnosticResultContext.Lxc => UrlHelper.Resources.VmUrl(long.Parse(data[3])),
            _ => "#",
        };
    }

    protected override async Task OnInitializedAsync()
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();

        _baseAddress = (await adminService[ClusterName].GetPveClientAsync()).BaseAddress;

        Items = await db.JobDetails.Where(a => a.JobResult.Id == ResultId && !a.IsIgnoredIssue).ToListAsync();
    }

    //private static string GetColor(Group group)
    //{
    //    var color = group.Data.Key switch
    //    {
    //        DiagnosticResultGravity.Info => Colors.InfoLighter,
    //        DiagnosticResultGravity.Warning => Colors.WarningLighter,
    //        DiagnosticResultGravity.Critical => Colors.DangerLighter,
    //        _ => Colors.Info,
    //    };

    //    return color;
    //}

    //private static void RowRender(RowRenderEventArgs<JobDetail> args)
    //{
    //    var color = args.Data.Gravity switch
    //    {
    //        DiagnosticResultGravity.Info => Colors.InfoLighter,
    //        DiagnosticResultGravity.Warning => Colors.WarningLighter,
    //        DiagnosticResultGravity.Critical => Colors.DangerLighter,
    //        _ => Colors.Info,
    //    };
    //    args.Attributes.Add("style", $"background-color: {color}; color: {Colors.Black}:");
    //}

    private void OnRender(DataGridRenderEventArgs<JobDetail> args)
    {
        if (args.FirstRender)
        {
            args.Grid!.Groups.Add(new()
            {
                Title = L["Gravity"],
                Property = nameof(JobDetail.Gravity)
            });

            StateHasChanged();
        }
    }

    private static void OnGroupRowRender(GroupRowRenderEventArgs args)
    {
        if (args.FirstRender) { args.Expanded = false; }
    }

    private async Task IgnoreIssueAsync(JobDetail item)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        var clusterName = (await db.JobResults.Where(a => a.Id == ResultId)
                                             .Select(a => a.ClusterName)
                                             .FirstOrDefaultAsync())!;

        if (await db.IgnoredIssues.FromClusterName(clusterName)
                                  .Where(a => a.IdResource == item.IdResource
                                                && a.Context == item.Context
                                                && a.Description == item.Description
                                                && a.SubContext == item.SubContext
                                                && a.Gravity == item.Gravity)
                                  .AnyAsync())
        {
            notificationService.Info(L["Issue already exists!"]);
        }
        else
        {
            await db.IgnoredIssues.AddAsync(new()
            {
                ClusterName = clusterName,
                IdResource = item.IdResource,
                Context = item.Context,
                Description = item.Description,
                SubContext = item.SubContext,
                Gravity = item.Gravity
            });
            await db.SaveChangesAsync();

            notificationService.Success(L["Issue created!"]);
        }
    }
}
