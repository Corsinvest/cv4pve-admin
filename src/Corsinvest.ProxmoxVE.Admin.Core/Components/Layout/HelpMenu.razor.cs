using Toolbelt.Blazor.HotKeys2;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components.Layout;

public partial class HelpMenu(ISettingsService settingsService, IReleaseService releaseService, DialogService dialogService)
{
    [Parameter] public HotKeysContext HotKeysContext { get; set; } = default!;

    private AppSettings AppSettings { get; set; } = default!;
    private ReleaseInfo? NewRelease { get; set; }

    protected override async Task OnInitializedAsync()
    {
        AppSettings = settingsService.GetAppSettings();
        NewRelease = await releaseService.NewReleaseIsAvailableAsync(false);
    }

    private async Task OnMenuClick(RadzenProfileMenuItem item)
    {
        if (item.Value?.ToString() == "shortcuts")
        {
            await ShowKeyboardShortcuts();
        }
    }

    private async Task ShowKeyboardShortcuts()
        => await dialogService.OpenAsync<KeyboardShortcutsDialog>(L["Keyboard Shortcuts"],
            new Dictionary<string, object>
            {
                { nameof(KeyboardShortcutsDialog.HotKeysContext), HotKeysContext }
            },
            new DialogOptions
            {
                Width = "400px",
                CloseDialogOnOverlayClick = true,
                ShowClose = true
            });
}
