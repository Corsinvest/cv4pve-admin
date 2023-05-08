/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.UI.ProxmoxVE.Cluster;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;

namespace Corsinvest.ProxmoxVE.Admin.VmUnlock;

public partial class RenderIndex
{
    [Inject] private IPveClientService PveClientService { get; set; } = default!;

    private Resources? RefResources { get; set; } = default!;
    private bool LoadingUnlock { get; set; }

    private async Task<IEnumerable<ClusterResource>> GetItems() => await Helper.GetVmLocks(await PveClientService.GetClientCurrentCluster());

    private async Task Unlock()
    {
        LoadingUnlock = true;
        if (await UIMessageBox.ShowQuestionAsync(L["Unlock"], L["Unlock VM/CT?"]))
        {
            await Helper.Unlock(await PveClientService.GetClientCurrentCluster(), RefResources!.DataGridManager.SelectedItems);
            RefResources!.DataGridManager.SelectedItems.Clear();
            await RefResources!.DataGridManager.Refresh();
        }
        LoadingUnlock = false;
    }
}