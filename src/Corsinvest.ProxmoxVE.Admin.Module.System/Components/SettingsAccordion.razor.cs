/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Module.System.Components;

public partial class SettingsAccordion(ISettingsService settingsService, NotificationService notificationService)
{
    [Parameter] public IEnumerable<SettingSection> Sections { get; set; } = [];

    private AppSettings AppSettings { get; set; } = default!;

    protected override void OnInitialized() => AppSettings = settingsService.GetAppSettings();

    private Dictionary<string, object> GetParameters() => new()
    {
        { "Settings", AppSettings },
        { "SettingsChanged", EventCallback.Factory.Create<AppSettings>(this, OnSettingsChanged) }
    };

    private void OnSettingsChanged(AppSettings settings)
    {
        AppSettings = settings;
        StateHasChanged();
    }

    private async Task SaveAsync()
    {
        await settingsService.SetAsync(AppSettings);
        notificationService.Success(L["Settings saved!"]);
    }
}
