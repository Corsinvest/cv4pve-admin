/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Commands;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;
using Mapster;
using Corsinvest.ProxmoxVE.Admin.Core.Commands.Vm;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components.ProxmoxVE.Vm.Snapshot;

public partial class Manager(IAdminService adminService,
                             CommandExecutor commandExecutor,
                             DialogService dialogService) : IRefreshableData, IClusterName, IDisposable
{
    [EditorRequired, Parameter] public IClusterResourceVm Vm { get; set; } = default!;
    [EditorRequired, Parameter] public string ClusterName { get; set; } = default!;
    [Parameter] public string Style { get; set; } = default!;
    //[Parameter] public bool ShowDetailProxmoxVE { get; set; }
    [Parameter] public bool CanCreate { get; set; }
    [Parameter] public bool CanEdit { get; set; }
    [Parameter] public bool CanDelete { get; set; }
    [Parameter] public bool CanRollback { get; set; }

    private bool AllowCalculateSnapshotSize { get; set; }
    private RadzenDataGrid<Data> DataGridRef { get; set; } = default!;
    private IEnumerable<Data> AllItems { get; set; } = default!;
    private IEnumerable<Data> Items { get; set; } = default!;
    private IList<Data> SelectedItems { get; set; } = [];
    private bool IsCalculateSnapshotSize { get; set; }
    private bool _validColumnClick;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);

    private class Data : VmSnapshot
    {
        [Display(Name = "Snapshots Size")]
        [DisplayFormat(DataFormatString = FormatHelper.DataFormatBytes)]
        public double SnapshotSize { get; set; }
    }

    protected override async Task OnInitializedAsync() => await RefreshDataAsync();

    public async Task RefreshDataAsync()
    {
        if (!await _refreshLock.WaitAsync(0)) { return; }
        try
        {
            await RefreshDataAsyncInt();
        }
        finally
        {
            try { _refreshLock?.Release(); } catch (ObjectDisposedException) { }
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

        Items = [.. AllItems.Where(a => a.Parent == "no-parent")];
        SelectedItems.Clear();

        if (AllowCalculateSnapshotSize)
        {
            IsCalculateSnapshotSize = true;
            await InvokeAsync(StateHasChanged);

            var disks = await clusterClient.CachedData.GetDisksInfoAsync(false);

            foreach (var item in AllItems)
            {
                item.SnapshotSize = disks.Where(a => a.VmId == Vm.VmId && a.Host == Vm.Node)
                                         .SelectMany(a => a.Snapshots)
                                         .Where(a => !a.Replication && a.Name == item.Name)
                                         .Sum(a => a.Size);
            }

            IsCalculateSnapshotSize = false;
        }

        await InvokeAsync(StateHasChanged);

        await DataGridRef.ExpandRows(AllItems);
        await InvokeAsync(StateHasChanged);
    }

    private void RowRender(RowRenderEventArgs<Data> args) => args.Expandable = AllItems.Any(e => e.Parent == args.Data!.Name);
    private void LoadChildData(DataGridLoadChildDataEventArgs<Data> args) => args.Data = AllItems.Where(e => e.Parent == args.Item!.Name);

    private async Task RollbackAsync()
    {
        if (await dialogService.ConfirmAsync(L["Are you sure?"],
                                               L["Rollback snapshot '{name}'", SelectedItems[0].Name],
                                               true))
        {
            await commandExecutor.ExecuteAsync(new VmRollbackSnapshotCommand(ClusterName,
                                                                             Vm.VmId,
                                                                             SelectedItems[0].Name));
            SelectedItems.Clear();
            await DataGridRef.Reload();
        }
    }

    private async Task DeleteAsync()
    {
        if (!CanDelete) { return; }
        if (await dialogService.ConfirmAsync(L["Are you sure?"], L["Delete selected snapshot"], true))
        {
            await commandExecutor.ExecuteAsync(new VmRemoveSnapshotCommand(ClusterName,
                                                                           Vm.VmId,
                                                                           SelectedItems[0].Name,
                                                                           Force: true));
            SelectedItems.Clear();
            await DataGridRef.Reload();
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
        if (_validColumnClick && CanEdit)
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

        var title = isNew
                        ? L["New"]
                        : L["Edit {0}", item.Name];

        if (await dialogService.OpenSideEditAsync<EditDialog>(title, isNew, item) != null)
        {
            if (isNew)
            {
                await commandExecutor.ExecuteAsync(new VmCreateSnapshotCommand(ClusterName,
                                                                               Vm.VmId,
                                                                               item.Name,
                                                                               item.Description,
                                                                               item.VmStatus));
            }
            else
            {
                await commandExecutor.ExecuteAsync(new VmUpdateSnapshotCommand(ClusterName,
                                                                               Vm.VmId,
                                                                               item.Name,
                                                                               item.Description));
            }

            await DataGridRef.Reload();
        }
    }

    public void Dispose()
    {
        _refreshLock?.Dispose();
        GC.SuppressFinalize(this);
    }
}
