/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Toolbelt.Blazor.HotKeys2;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components.Layout;

public partial class HelpMenu(ISettingsService settingsService,
                               IReleaseService releaseService,
                               DialogService dialogService)
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
            await ShowKeyboardShortcutsAsync();
        }
        else if (item.Value?.ToString() == "release-notes")
        {
            await ShowReleaseNotesAsync();
        }
    }

    private async Task ShowKeyboardShortcutsAsync()
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

    private async Task ShowReleaseNotesAsync()
        => await dialogService.OpenSideExAsync<ReleaseNotesDialog>(L["Release Notes"],
                                                                   [],
                                                                   new DialogOptions
                                                                   {
                                                                       CloseDialogOnOverlayClick = true,
                                                                       ShowClose = true
                                                                   });
}
