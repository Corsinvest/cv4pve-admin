/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.BackgroundJob;
using Corsinvest.AppHero.Core.UI;
using Corsinvest.ProxmoxVE.Admin.Core.Services;
using Corsinvest.ProxmoxVE.Admin.ReplicationTrend.Repository;

namespace Corsinvest.ProxmoxVE.Admin.ReplicationTrend.Components;

public partial class Jobs
{
    [Inject] private IJobService JobService { get; set; } = default!;
    [Inject] private IDataGridManagerRepository<ReplicationResult> DataGridManager { get; set; } = default!;
    [Inject] private IPveClientService PveClientService { get; set; } = default!;
    [Inject] private IOptionsSnapshot<Options> Options { get; set; } = default!;

    private bool ShowDialog { get; set; }
    private string DialogContent { get; set; } = default!;
    private string ClusterName { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        DataGridManager.Title = L["Jobs"];
        DataGridManager.DefaultSort = new() { [nameof(ReplicationResult.Start)] = true };
        DataGridManager.QueryAsync = async () => await DataGridManager.Repository.ListAsync(new ReplicationResultSpec(ClusterName));

        try
        {
            ClusterName = await PveClientService.GetCurrentClusterNameAsync();
        }
        catch { }
    }

    private async Task Scan()
    {
        if (await UIMessageBox.ShowQuestionAsync(L["Scan"], L["Execute Scan?"]))
        {
            JobService.Schedule<Job>(a => a.Scan(ClusterName), TimeSpan.FromSeconds(10));
            UINotifier.Show(L["Scan jobs started!"], UINotifierSeverity.Info);
        }
    }

    private void ShowLog(ReplicationResult item)
    {
        DialogContent = item.Log;
        ShowDialog = true;
    }

    private static string CellClassFunc(ReplicationResult item)
        => string.IsNullOrEmpty(item.Error)
                ? string.Empty
                : "mud-theme-error";
}