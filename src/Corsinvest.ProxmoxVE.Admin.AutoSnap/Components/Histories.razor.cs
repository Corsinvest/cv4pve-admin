/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.BaseUI.DataManager;
using Corsinvest.ProxmoxVE.Admin.AutoSnap.Models;
using Corsinvest.ProxmoxVE.Admin.AutoSnap.Repository;

namespace Corsinvest.ProxmoxVE.Admin.AutoSnap.Components;

public partial class Histories
{
    [Parameter] public string Height { get; set; } = default!;
    [Parameter] public bool ShowOnlyError { get; set; }
    [Parameter] public int JobId { get; set; }

    [Inject] private IDataGridManagerRepository<AutoSnapJobHistory> DataGridManager { get; set; } = default!;
    [Inject] private IPveClientService PveClientService { get; set; } = default!;

    private bool ShowDialog { get; set; }
    private string DialogContent { get; set; } = default!;

    protected override void OnInitialized()
    {
        DataGridManager.Title = L["Histories"];
        DataGridManager.DefaultSort = new() { [nameof(AutoSnapJobHistory.Start)] = true };

        DataGridManager.QueryAsync = async ()
            => await DataGridManager.Repository.ListAsync(new AutoSnapJobHistorySpec(await PveClientService.GetCurrentClusterNameAsync(), JobId, ShowOnlyError));
    }

    private void ShowLog(AutoSnapJobHistory item)
    {
        DialogContent = item.Log;
        ShowDialog = true;
    }
}
