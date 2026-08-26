/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Commands;
using Corsinvest.ProxmoxVE.Admin.Core.Commands.Vm;
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;
using Mapster;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components.ProxmoxVE.Vm.Snapshot;

public partial class Manager(IAdminService adminService,
                             IUiCommandExecutor uiExecutor,
                             DialogService dialogService) : IRefreshableData, IClusterName, IDisposable
{
    [EditorRequired, Parameter] public IClusterResourceVm Vm { get; set; } = default!;
    [EditorRequired, Parameter] public string ClusterName { get; set; } = default!;
    [Parameter] public string Style { get; set; } = default!;

    [Parameter] public bool ShowOrphans { get; set; }

    private bool CanCreate { get; set; }
    private bool CanEdit { get; set; }
    private bool CanDelete { get; set; }
    private bool CanRollback { get; set; }
    private bool AllowCalculateSnapshotSize { get; set; }
    private RadzenDataGrid<Data> DataGridRef { get; set; } = default!;
    private IEnumerable<Data> AllItems { get; set; } = default!;
    private IEnumerable<Data> Items { get; set; } = default!;
    private IList<Data> SelectedItems { get; set; } = [];
    private bool IsCalculateSnapshotSize { get; set; }
    private bool _validColumnClick;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private bool _disposed;

    private class Data : VmSnapshot
    {
        [Display(Name = "Snapshots Size")]
        [DisplayFormat(DataFormatString = FormatHelper.DataFormatBytes)]
        public double SnapshotSize { get; set; }

        public bool IsOrphan { get; set; }
    }

    protected override async Task OnInitializedAsync()
    {
        var hasSnapshot = await PermissionService.HasVmAsync(ClusterName, ClusterPermissions.Vm.Snapshot, Vm.VmId);

        CanCreate = hasSnapshot;
        CanEdit = hasSnapshot;
        CanDelete = hasSnapshot;
        CanRollback = await PermissionService.HasVmAsync(ClusterName, ClusterPermissions.Vm.SnapshotRallback, Vm.VmId);

        await RefreshDataAsync();
    }

    public async Task RefreshDataAsync()
    {
        if (_disposed || !await _refreshLock.WaitAsync(0)) { return; }
        try
        {
            await RefreshDataAsyncInt();
        }
        finally
        {
            if (!_disposed) { _refreshLock?.Release(); }
        }
    }

    private async Task RefreshDataAsyncInt()
    {
        AllItems = [];
        Items = [];

        var clusterClient = adminService[ClusterName];

        AllowCalculateSnapshotSize = clusterClient.Settings.AllowCalculateSnapshotSize;

        AllItems = [.. (await clusterClient.CachedData.GetSnapshotsAsync(Vm.Node, Vm.VmType, Vm.VmId, false))
                            .AsQueryable()
                            .ProjectToType<Data>()];

        // Roots are snapshots without a parent, plus orphans: Proxmox keeps the parent name
        // on the children when a snapshot is removed, so a guest whose original root has been
        // rotated away by retention ends up with no "no-parent" entry and nothing to render.
        var names = AllItems.Select(a => a.Name).ToHashSet();
        Items = [.. AllItems.Where(a => a.Parent == "no-parent" || !names.Contains(a.Parent))];
        SelectedItems.Clear();

        if (AllowCalculateSnapshotSize)
        {
            IsCalculateSnapshotSize = true;
            await InvokeAsync(StateHasChanged);

            var disks = await clusterClient.CachedData.GetDiskSnapshotInfosAsync(false);

            foreach (var item in AllItems)
            {
                item.SnapshotSize = DiskSnapshotHelper.CalculateSnapshot(Vm.Node, Vm.VmId, item.Name, disks);
            }

            if (ShowOrphans)
            {
                // Top level only: AllItems feeds the child lookup, and the same instance in both
                // would render twice among siblings, which Blazor rejects as a duplicate key.
                Items = [.. GetOrphans(names, disks), .. Items];
            }

            IsCalculateSnapshotSize = false;
        }

        await InvokeAsync(StateHasChanged);

        await DataGridRef.ExpandRows(AllItems);
        await InvokeAsync(StateHasChanged);
    }

    private IEnumerable<Data> GetOrphans(HashSet<string> knownNames, IEnumerable<DiskSnapshotInfo> disks)
        => disks.Where(a => a.VmId == Vm.VmId && a.Host == Vm.Node)
                .SelectMany(a => a.Snapshots)
                .Where(a => !a.Replication && !knownNames.Contains(a.Name))
                .GroupBy(a => a.Name)
                .Select(a => new Data
                {
                    Name = a.Key,
                    SnapshotSize = a.Sum(b => b.Size),
                    IsOrphan = true
                })
                .OrderByDescending(a => a.SnapshotSize);

    private void RowRender(RowRenderEventArgs<Data> args)
    {
        if (args.Data!.IsOrphan)
        {
            args.Expandable = false;
            args.Attributes["class"] = "rz-background-color-warning-lighter";
            return;
        }
        args.Expandable = AllItems.Any(e => !e.IsOrphan && e.Parent == args.Data.Name);
    }

    private void LoadChildData(DataGridLoadChildDataEventArgs<Data> args)
        => args.Data = AllItems.Where(e => !e.IsOrphan && e.Parent == args.Item!.Name);

    private bool SelectionIsActionable => SelectedItems.Any() && !SelectedItems[0].IsOrphan;

    private async Task RollbackAsync()
    {
        if (!CanRollback || !SelectionIsActionable) { return; }
        if (await dialogService.ConfirmAsync(L["Are you sure?"],
                                               L["Rollback snapshot '{name}'", SelectedItems[0].Name],
                                               true))
        {
            var result = await uiExecutor.ExecuteAndNotifyAsync(new VmRollbackSnapshotCommand(ClusterName,
                                                                                                Vm.VmId,
                                                                                                SelectedItems[0].Name));
            if (result.IsSuccess)
            {
                SelectedItems.Clear();
                await DataGridRef.Reload();
            }
        }
    }

    private async Task DeleteAsync()
    {
        if (!CanDelete || !SelectionIsActionable) { return; }
        if (await dialogService.ConfirmAsync(L["Are you sure?"], L["Delete selected snapshot"], true))
        {
            var result = await uiExecutor.ExecuteAndNotifyAsync(new VmRemoveSnapshotCommand(ClusterName,
                                                                                             Vm.VmId,
                                                                                             SelectedItems[0].Name,
                                                                                             Force: true));
            if (result.IsSuccess)
            {
                SelectedItems.Clear();
                await DataGridRef.Reload();
            }
        }
    }

    private async Task KeyDownAsync(KeyboardEventArgs e)
    {
        if (SelectedItems.Any() && e.IsForDelete()) { await DeleteAsync(); }
        else if (SelectedItems.Any() && e.IsForEdit()) { await RowSelectAsync(SelectedItems[0]); }
        else if (e.IsForNew()) { await AddAsync(); }
    }

    private async Task RowSelectAsync(Data item)
    {
        if (_validColumnClick && CanEdit && !item.IsOrphan)
        {
            await ShowEditorAsync(new SnapshotModel
            {
                Name = item.Name,
                Description = item.Description,
                VmStatus = item.VmStatus
            }, false);
        }
    }

    private void CellClick(DataGridCellMouseEventArgs<Data> e) => _validColumnClick = nameof(Data.Name) == e.Column!.Property;
    private async Task AddAsync()
    {
        if (!CanCreate) { return; }
        await ShowEditorAsync(new(), true);
    }

    private async Task ShowEditorAsync(SnapshotModel item, bool isNew)
    {
        item.HasVmStatus = Vm.VmType == VmType.Qemu;

        if (await dialogService.OpenSideEditAsync<EditDialog>(isNew
                                                                ? L["New"]
                                                                : L["Edit {0}", item.Name],
                                                              isNew
                                                                ? EditDialogMode.Create
                                                                : EditDialogMode.Edit,
                                                              item) != null)
        {
            var result = isNew
                ? await uiExecutor.ExecuteAndNotifyAsync(new VmCreateSnapshotCommand(ClusterName,
                                                                                       Vm.VmId,
                                                                                       item.Name,
                                                                                       item.Description,
                                                                                       item.VmStatus))
                : await uiExecutor.ExecuteAndNotifyAsync(new VmUpdateSnapshotCommand(ClusterName,
                                                                                       Vm.VmId,
                                                                                       item.Name,
                                                                                       item.Description));

            if (result.IsSuccess)
            {
                await DataGridRef.Reload();
            }
        }
    }

    public void Dispose()
    {
        _disposed = true;
        _refreshLock?.Dispose();
        GC.SuppressFinalize(this);
    }
}
