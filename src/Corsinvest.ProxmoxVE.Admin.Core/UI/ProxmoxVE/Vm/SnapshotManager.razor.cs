/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.Security.Auth.Permissions;
using Corsinvest.ProxmoxVE.Api;
using Corsinvest.ProxmoxVE.Api.Extension.Utils;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;

namespace Corsinvest.ProxmoxVE.Admin.Core.UI.ProxmoxVE.Vm;

public partial class SnapshotManager
{
    [Parameter] public string Height { get; set; } = default!;
    [EditorRequired][Parameter] public IClusterResourceVm Vm { get; set; } = default!;
    [EditorRequired][Parameter] public PveClient PveClient { get; set; } = default!;
    [Parameter] public PermissionsRead Permissions { get; set; } = default!;
    [Parameter] public bool ShowDetailProxmoxVE { get; set; }
    [Parameter] public bool CanCreate { get; set; }
    [Parameter] public bool CanDelete { get; set; }
    [Parameter] public bool CanRollback { get; set; }

    [Inject] private IDataGridManager<VmSnapshot> DataGridManager { get; set; } = default!;

    protected override void OnInitialized()
    {
        DataGridManager.Title = L["Snapshots"];
        DataGridManager.DefaultSort = new() { [nameof(VmSnapshot.Time)] = false };
        DataGridManager.QueryAsync = async () => await SnapshotHelper.GetSnapshots(PveClient, Vm.Node, Vm.VmType, Vm.VmId);

        DataGridManager.SaveAsync = async (item, isNew) =>
        {
            await SnapshotHelper.CreateSnapshot(PveClient, Vm.Node, Vm.VmType, Vm.VmId, item.Name, item.Description, item.VmStatus, 30000);
            return true;
        };

        DataGridManager.DeleteAsync = async (items) =>
        {
            foreach (var item in items)
            {
                await SnapshotHelper.RemoveSnapshot(PveClient, Vm.Node, Vm.VmType, Vm.VmId, item.Name, 30000, true);
            }

            return true;
        };
    }

    private async Task RollbackAsync()
    {
        if (await UIMessageBox.ShowQuestionAsync(L["Rollback Snapshot"], L["Rollback Snapshot?"]))
        {
            await SnapshotHelper.RollbackSnapshot(PveClient, Vm.Node, Vm.VmType, Vm.VmId, DataGridManager.SelectedItem.Name, 30000);
            await DataGridManager.Refresh();
        }
    }
}