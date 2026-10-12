/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Commands;
using Corsinvest.ProxmoxVE.Admin.Core.Commands.Vm;
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Node;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components.ProxmoxVE.Vm.Backup;

public partial class Manager(DialogService dialogService,
                             IAdminService adminService,
                             IUiCommandExecutor uiExecutor) : IRefreshableData, IClusterName
{
    [EditorRequired, Parameter] public string ClusterName { get; set; } = default!;
    [EditorRequired, Parameter] public IClusterResourceVm Vm { get; set; } = default!;
    [Parameter] public string Style { get; set; } = default!;

    private bool CanRestoreFile { get; set; }
    private bool CanDelete { get; set; }
    private bool CanEdit { get; set; }
    private bool CanBackup { get; set; }
    private bool IsLoading { get; set; }
    private RadzenDataGrid<NodeStorageContent> DataGridRef { get; set; } = default!;
    private IEnumerable<NodeStorageContent> Items { get; set; } = default!;
    private IList<NodeStorageContent> SelectedItems { get; set; } = [];
    private NodeStorageContent SelectedItem => SelectedItems[0];
    private IEnumerable<NodeStorage> Storages { get; set; } = [];
    private string StorageName { get; set; } = default!;
    private bool _validColumnClick;

    protected override async Task OnInitializedAsync()
    {
        var hasBackup = await PermissionService.HasVmAsync(ClusterName, ClusterPermissions.Vm.Backup, Vm.VmId);

        CanBackup = hasBackup;
        CanEdit = hasBackup;
        CanDelete = hasBackup;
        CanRestoreFile = await PermissionService.HasVmAsync(ClusterName, ClusterPermissions.Vm.BackupRestoreFile, Vm.VmId);

        var client = await adminService[ClusterName].GetPveClientAsync();
        Storages = [.. (await client.Nodes[Vm.Node].Storage.GetAsync(content: "backup", enabled: true))
                            .Where(a => a.Enabled && a.Active)
                            .OrderBy(a => a.Storage)];

        StorageName = Storages.FirstOrDefault()?.Storage!;

        await RefreshDataAsync();
    }

    public async Task RefreshDataAsync()
    {
        IsLoading = true;
        SelectedItems.Clear();

        var client = await adminService[ClusterName].GetPveClientAsync();
        Items = await client.Nodes[Vm.Node]
                            .Storage[StorageName]
                            .Content
                            .GetAsync("backup", Convert.ToInt32(Vm.VmId));

        IsLoading = false;
    }

    private async Task KeyDownAsync(KeyboardEventArgs e)
    {
        if (SelectedItems.Any() && e.IsForDelete()) { await DeleteAsync(); }
        else if (SelectedItems.Any() && e.IsForEdit()) { await RowSelectAsync(SelectedItems[0]); }
        else if (e.IsForNew()) { await AddAsync(); }
    }

    private async Task RowSelectAsync(NodeStorageContent item)
    {
        if (_validColumnClick && CanEdit) { await ShowEditorAsync(item); }
    }

    private void CellClick(DataGridCellMouseEventArgs<NodeStorageContent> e)
        => _validColumnClick = nameof(NodeStorageContent.FileName) == e.Column!.Property;

    private async Task DeleteAsync()
    {
        if (!CanDelete) { return; }
        if (await dialogService.ConfirmAsync(L["Are you sure?"], L["Delete selected backup"], true))
        {
            await uiExecutor.ExecuteAndNotifyAsync(new VmDeleteBackupCommand(ClusterName, Vm.VmId, SelectedItem.Storage, SelectedItem.Volume));

            SelectedItems.Clear();
            await DataGridRef.Reload();
        }
    }

    private Task RestoreFileAsync()
        => dialogService.OpenSideExAsync<FileRestoreDialog>(L["Restore file {0}", SelectedItem.Volume],
                                                            new()
                                                            {
                                                                [nameof(FileRestoreDialog.ClusterName)] = ClusterName,
                                                                [nameof(FileRestoreDialog.Node)] = Vm.Node,
                                                                [nameof(FileRestoreDialog.Storage)] = SelectedItem.Storage,
                                                                [nameof(FileRestoreDialog.Volume)] = SelectedItem.Volume
                                                            },
                                                            new()
                                                            {
                                                                CloseDialogOnOverlayClick = true,
                                                                Width = "800px"
                                                            });

    private async Task ShowEditorAsync(NodeStorageContent item)
    {
        if (await dialogService.OpenSideEditAsync<EditDialog>(L["Edit"], EditDialogMode.Edit, item) != null)
        {
            await uiExecutor.ExecuteAndNotifyAsync(new VmUpdateBackupCommand(ClusterName,
                                                                             Vm.VmId,
                                                                             item.Storage,
                                                                             item.Volume,
                                                                             item.Notes,
                                                                             item.Protected));
            await RefreshDataAsync();
        }
    }

    private async Task ShowConfigurationAsync()
    {
        var client = await adminService[ClusterName].GetPveClientAsync();

        await dialogService.OpenSideLogAsync(L["Configuration {0}", SelectedItem.Volume],
                                                    (await client.Nodes[Vm.Node]
                                                                 .Vzdump
                                                                 .Extractconfig
                                                                 .Extractconfig(SelectedItem.Volume))
                                                    .ToModel<string>());
    }

    private async Task AddAsync()
    {
        if (!CanBackup) { return; }

        var item = new BackupModel
        {
            Storage = StorageName,
            Storages = [.. Storages.Select(a => a.Storage)]
        };

        if (await dialogService.OpenSideEditAsync<NewDialog>(L["New backup"], EditDialogMode.Create, item) != null)
        {
            await uiExecutor.ExecuteAndNotifyAsync(new VmBackupCommand(ClusterName,
                                                                       Vm.VmId,
                                                                       item.Storage,
                                                                       item.Mode,
                                                                       item.Compress,
                                                                       item.Protected,
                                                                       string.IsNullOrWhiteSpace(item.Notes) ? null : item.Notes));
        }
    }
}
