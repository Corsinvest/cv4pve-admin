/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Module.AutoSnap.Components;

public partial class Results(IDbContextFactory<ModuleDbContext> dbContextFactory,
                             DialogService dialogService) : IClusterName
{
    [CascadingParameter(Name = nameof(ClusterName))] public string ClusterName { get; set; } = default!;
    [Parameter] public bool ShowOnlyError { get; set; }
    [Parameter] public int? JobId { get; set; }

    private IList<Data> SelectedItems { get; set; } = [];
    private bool _validColumnClick;
    private RadzenDataGrid<Data> DataGridRef { get; set; } = default!;
    private ResultLoadData<Data> ResultLoadData { get; set; } = new(null!, -1, null);

    private class Data : JobResult;

    private async Task LoadDataAsync(LoadDataArgs args)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        ResultLoadData = await DataGridRef.LoadDataAsync(db.Results
                                                           .Where(a => a.Job.Id == JobId, JobId != null)
                                                           .Where(a => !a.Status, ShowOnlyError),
                                                         args,
                                                         a => new Data
                                                         {
                                                             Id = a.Id,
                                                             SnapName = a.SnapName,
                                                             Status = a.Status,
                                                             Start = a.Start,
                                                             End = a.End
                                                         },
                                                         ResultLoadData.Filter);
    }

    private void CellClick(DataGridCellMouseEventArgs<Data> e)
        => _validColumnClick = new[] { nameof(Data.SnapName) }.Contains(e.Column!.Property);

    private async Task RowSelectAsync(Data item)
    {
        if (_validColumnClick)
        {
            await using var db = await dbContextFactory.CreateDbContextAsync();
            await dialogService.OpenSideLogAsync(L["Job Log '{0}'", item.Start],
                                                    (await db.Results
                                                             .Where(a => a.Id == item.Id)
                                                             .Select(a => a.Logs)
                                                             .FirstOrDefaultAsync())!);
        }
    }

    private async Task KeyDownAsync(KeyboardEventArgs e)
    {
        if (SelectedItems.Any() && e.IsForDelete()) { await DeleteAsync(); }
        else if (SelectedItems.Any() && e.IsForEdit()) { await RowSelectAsync(SelectedItems[0]); }
        else if (e.IsForNew()) { }
    }

    private async Task DeleteAsync()
    {
        if (!await HasPermissionAsync(ClusterName, Module.Permissions.Results.Delete)) { return; }

        if (await dialogService.ConfirmAsync(L["Are you sure?"], L["Delete selected row"], true))
        {
            await using var db = await dbContextFactory.CreateDbContextAsync();
            await db.Results.DeleteAsync([.. SelectedItems.Select(a => a.Id)]);
            SelectedItems.Clear();
            await DataGridRef.Reload();
        }
    }
}
