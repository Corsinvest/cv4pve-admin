/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Ardalis.Specification;
using Corsinvest.AppHero.Core.BackgroundJob;
using Corsinvest.AppHero.Core.UI;
using Corsinvest.ProxmoxVE.Admin.Core.Services;
using Corsinvest.ProxmoxVE.Admin.VzDumpTrend.Repository;

namespace Corsinvest.ProxmoxVE.Admin.VzDumpTrend.Components;

public partial class Jobs
{
    [Inject] private IJobService JobService { get; set; } = default!;
    [Inject] private IDataGridManagerRepository<VzDumpDetail> DataGridManager { get; set; } = default!;
    [Inject] private IPveClientService PveClientService { get; set; } = default!;
    [Inject] private IOptionsSnapshot<Options> Options { get; set; } = default!;

    private bool ShowDialog { get; set; }
    private string DialogTitle { get; set; } = default!;
    private string DialogContent { get; set; } = default!;
    private string ClusterName { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            ClusterName = await PveClientService.GetCurrentClusterName();
        }
        catch { }

        DataGridManager.Title = L["Jobs"];
        DataGridManager.DefaultSort = new() { [nameof(VzDumpDetail.Start)] = false };
        DataGridManager.QueryAsync = async () => await DataGridManager.Repository.ListAsync(new VzDumpDetailSpec(ClusterName));
    }

    private async Task Scan()
    {
        if (await UIMessageBox.ShowQuestionAsync(L["Scan"], L["Execute Scan?"]))
        {
            JobService.Schedule<Job>(a => a.Scan(ClusterName), TimeSpan.FromSeconds(10));
            UINotifier.Show(L["Scan jobs started!"], UINotifierSeverity.Info);
        }
    }

    private void ShowLog(VzDumpDetail item, bool jobLog)
    {
        if (jobLog)
        {
            DialogTitle = L["Job Vm: {0}", item.VmId!];
            DialogContent = Helper.ParserVzDumpFromTaskLog(item.Task)
                                  .Where(a => a.VmId == item.VmId)
                                  .First()
                                  .Logs
                                  .JoinAsString(Environment.NewLine);
        }
        else
        {
            DialogTitle = L["Full Task"];
            DialogContent = item.Task.Log!;
        }

        ShowDialog = true;
    }

    private static string CellClassFunc(VzDumpDetail item)
        => string.IsNullOrEmpty(item.Error)
                ? string.Empty
                : "mud-theme-error";
}