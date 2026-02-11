namespace Corsinvest.ProxmoxVE.Admin.Module.AutoSnap.Components;

public partial class Hooks
{
    //    private IQueryable<JobHook> _items = default!;
    //    private IQueryable<JobHook> Items { get; set; } = default!;
    //    private IList<JobHook> SelectedItems { get; set; } = [];
    //    private JobHook SelectedItem => SelectedItems[0];
    //    private bool _validColumnClick;

    //    private async Task RowSelect(JobHook item)
    //    {
    //        if (_validColumnClick) { await ShowEditorAsync(item, false); }
    //    }

    //    private void CellClick(DataGridCellMouseEventArgs<JobHook> e)
    //    {
    //        var properties = new[] { nameof(JobHook.Description) };
    //        _validColumnClick = properties.Contains(e.Column.Property);
    //    }

    //    //private async Task<bool> ConfirmAsync(string prefix)
    //    //    => await DialogService.Confirm(L["Are you sure?"],
    //    //                                   L[$"{prefix} Job '{SelectedItem.Id}' with label '{SelectedItem.Description}'"],
    //    //                                   new ConfirmOptions()
    //    //                                   {
    //    //                                       OkButtonText = L["Yes"],
    //    //                                       CancelButtonText = L["No"],
    //    //                                   }) ?? false;

    //    private async Task DeleteAsync()
    //    {
    //        //var ret = await ConfirmAsync("Delete");
    //    }

    //    private async Task TestAsync()
    //    {
    //        //var ret = await ConfirmAsync("Test");
    //    }

    //    private async Task AddAsync()
    //    {
    //        await ShowEditorAsync(new(), false);
    //    }

    //    public void Export()
    //    {
    //        //navigationManager.NavigateTo(query != null ? query.ToUrl($"/export/northwind/{table}/{type}") : $"/export/northwind/{table}/{type}", true);

    //        //service.Export("OrderDetails", type, new Query()
    //        //{
    //        //    OrderBy = grid.Query.OrderBy,
    //        //    Filter = grid.Query.Filter,
    //        //    Select = string.Join(",", grid.ColumnsCollection.Where(c => c.GetVisible() && !string.IsNullOrEmpty(c.Property))
    //        //                .Select(c => c.Property.Contains(".") ? $"{c.Property} as {c.Property.Replace(".", "_")}" : c.Property))
    //        //});
    //    }

    //    private async Task ShowEditorAsync(JobHook item, bool isNew)
    //    {
    //        //await DialogService.OpenSideexAsync<HookDialog>(L[IsNew ? "New" : "Edit"],
    //        //                                              new Dictionary<string, object>() { { nameof(HookDialog.Model), item } },
    //        //                                              new SideDialogOptions
    //        //                                              {
    //        //                                                  //CloseDialogOnOverlayClick = true,
    //        //                                                 // Position = DialogPosition.Right,
    //        //                                                  //ShowMask = true,
    //        //                                                  //ShowClose = false,
    //        //                                              });

    //        //await DialogService.OpenAsync<HookDialog>(L[isNew ? "New" : "Edit"],
    //        //                                      new Dictionary<string, object>() {
    //        //                                          { nameof(HookDialog.Model), item },
    //        //                                          { nameof(HookDialog.IsNew), isNew }
    //        //                                      },
    //        //                                      new DialogOptions
    //        //                                      {
    //        //                                          CssClass = "rz-dialog-side rz-dialog-side-position-right",
    //        //                                          CloseDialogOnOverlayClick = true,
    //        //                                          // Position = DialogPosition.Right,
    //        //                                          //ShowMask = true,
    //        //                                          //ShowClose = false,
    //        //                                      });

    //    }

    //    //private void OnSearch(string? value)
    //    //    => Items = string.IsNullOrEmpty(value)
    //    //                ? _items
    //    //                : _items.FilterAllProperties(value);

    //    protected override void OnInitialized()
    //    {
    //        _items = Enumerable.Range(1, 100).Select(a => new JobHook
    //        {
    //            //Id = a,
    //            Description = $"Description {a}"
    //        });

    //        Items = _items;

    //        base.OnInitialized();
    //    }
}
