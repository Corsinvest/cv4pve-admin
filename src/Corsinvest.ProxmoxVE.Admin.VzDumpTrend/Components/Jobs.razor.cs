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

    private bool ShowDialog { get; set; }
    private string DialogTitle { get; set; } = default!;
    private string DialogContent { get; set; } = default!;

    protected override void OnInitialized()
    {
        DataGridManager.Title = L["Jobs"];
        DataGridManager.DefaultSort = new() { [nameof(VzDumpDetail.Start)] = false };
        DataGridManager.QueryAsync = async () =>
        {
            var clusterName = await PveClientService.GetCurrentClusterName();
            return await DataGridManager.Repository.ListAsync(new VzDumpDetailSpec(clusterName));
        };
    }

    private async Task Scan()
    {
        if (await UIMessageBox.ShowQuestionAsync(L["Scan"], L["Execute Scan?"]))
        {
            var clusterName = await PveClientService.GetCurrentClusterName();
            JobService.Schedule<Job>(a => a.Scan(clusterName), TimeSpan.FromSeconds(10));
            UINotifier.Show(L["Scan jobs started!"], UINotifierSeverity.Info);
        }
    }

    private void ShowLog(VzDumpDetail item, bool jobLog)
    {
        DialogTitle = jobLog
                        ? $"Job Vm: {item.VmId}"
                        : "Full Task";
        if (jobLog)
        {
            DialogContent = Helper.ParserVzDumpFromTaskLog(item.Task)
                                  .Where(a => a.VmId == item.VmId)
                                  .First()
                                  .Logs
                                  .JoinAsString(Environment.NewLine);
        }
        else
        {
            DialogContent = item.Task.Log!;
        }

        ShowDialog = true;
    }

    private static string CellClassFunc(VzDumpDetail item)
        => string.IsNullOrEmpty(item.Error)
                ? string.Empty
                : "mud-theme-error";
}