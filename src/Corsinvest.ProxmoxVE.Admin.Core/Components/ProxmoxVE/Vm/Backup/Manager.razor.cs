/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Node;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components.ProxmoxVE.Vm.Backup;

public partial class Manager(DialogService dialogService,
                             IAdminService adminService) : IRefreshableData, IClusterName
{
    [EditorRequired, Parameter] public string ClusterName { get; set; } = default!;
    [EditorRequired, Parameter] public IClusterResourceVm Vm { get; set; } = default!;
    [Parameter] public string Style { get; set; } = default!;

    private bool CanRestore { get; set; }
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
        CanBackup = await PermissionService.HasVmAsync(ClusterName, ClusterPermissions.Vm.Backup, Vm.VmId);
        CanDelete = await PermissionService.HasVmAsync(ClusterName, ClusterPermissions.Vm.Backup, Vm.VmId);
        CanEdit = CanDelete;
        CanRestore = await PermissionService.HasVmAsync(ClusterName, ClusterPermissions.Vm.BackupRestore, Vm.VmId);
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
            var client = await adminService[ClusterName].GetPveClientAsync();
            await client.Nodes[Vm.Node]
                        .Storage[SelectedItem.Storage]
                        .Content[SelectedItem.Volume]
                        .Delete();

            SelectedItems.Clear();
            await DataGridRef.Reload();
        }
    }

    private async Task RestoreFileAsync()
        => await dialogService.OpenSideExAsync<FileRestoreDialog>(L["Restore file {0}", SelectedItem.Volume],
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
            var client = await adminService[ClusterName].GetPveClientAsync();

            await client.Nodes[Vm.Node]
                        .Storage[item.Storage]
                        .Content[item.Volume]
                        .Updateattributes(notes: item.Notes, protected_: item.Protected);

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
        await dialogService.Alert("Not available");

        await Task.CompletedTask;
    }

    private async Task RestoreAsync()
    {
        if (!CanRestore) { return; }
        await dialogService.Alert("Not available");

        await Task.CompletedTask;
    }
}
