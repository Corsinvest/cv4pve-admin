using Corsinvest.ProxmoxVE.Admin.Core.Notifier;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components.Settings;

public partial class NotifierSettingsDataGrid<TSettings>(INotifierService notifierService,
                                                         ISettingsService settingsService,
                                                         IServiceProvider serviceProvider,
                                                         NotificationService notificationService,
                                                         DialogService dialogService)
    where TSettings : NotifierConfiguration
{
    [Parameter] public RenderFragment GridColumns { get; set; } = default!;
    [Parameter] public RenderFragment<TSettings> EditContent { get; set; } = default!;
    [Parameter] public string? EditContentWidth { get; set; }
    [Parameter] public EventCallback<TSettings> OpenEdit { get; set; }

    private List<TSettings> Items { get; set; } = [];
    private IList<TSettings> SelectedItems { get; set; } = [];
    private RadzenDataGrid<TSettings> DataGridRef { get; set; } = default!;

    private bool _validColumnClick;

    protected override void OnInitialized() => RefreshData();
    private void RefreshData() => Items = [.. notifierService.Get(typeof(TSettings)).Cast<TSettings>()];

    private async Task RefreshDataAsync()
    {
        RefreshData();
        await DataGridRef.Reload();
    }

    private void Save() => notifierService.SetAsync(typeof(TSettings), Items);

    private async Task RowSelectAsync(TSettings item)
    {
        if (_validColumnClick) { await ShowEditorAsync(item, false); }
    }

    private void CellClick(DataGridCellMouseEventArgs<TSettings> e)
        => _validColumnClick = e.Column?.Property == nameof(NotifierConfiguration.Name);

    private async Task AddAsync() => await ShowEditorAsync(Activator.CreateInstance<TSettings>(), true);

    private async Task DeleteAsync()
    {
        if (await dialogService.ConfirmAsync(L["Are you sure?"], L["Delete"], true))
        {
            Items.Remove(SelectedItems[0]);
            Save();
            await DataGridRef.Reload();
        }
    }

    private async Task TestAsync()
    {
        var result = await SelectedItems[0].SendTestAsync(settingsService.GetAppSettings().AppName, serviceProvider);
        if (result.IsSuccess)
        {
            notificationService.Notify(new()
            {
                Severity = Radzen.NotificationSeverity.Info,
                Summary = L["Test done successfully!"]
            });
        }
        else
        {
            notificationService.Notify(new()
            {
                Severity = Radzen.NotificationSeverity.Error,
                Summary = result.Errors.Select(a => a.Message).JoinAsString(","),
                Duration = 10000
            });
        }
    }

    private async Task ShowEditorAsync(TSettings item, bool isNew)
    {
        if (OpenEdit.HasDelegate) { await OpenEdit.InvokeAsync(item); }

        var ret = await dialogService.OpenSideExAsync<NotifierSettingsDialog<TSettings>>(L[isNew ? "New" : "Edit"],
                                                                                             new()
                                                                                             {
                                                                                                 [nameof(NotifierSettingsDialog<>.Model)] = item,
                                                                                                 [nameof(NotifierSettingsDialog<>.IsNew)] = isNew,
                                                                                                 [nameof(NotifierSettingsDialog<>.EditContent)] = EditContent
                                                                                             },
                                                                                             new()
                                                                                             {
                                                                                                 CloseDialogOnOverlayClick = true,
                                                                                                 Width = EditContentWidth
                                                                                             }) != null;
        if (ret == true)
        {
            if (isNew)
            {
                Items.Add(item);
                await DataGridRef.Reload();
            }
            Save();
        }
    }
}
