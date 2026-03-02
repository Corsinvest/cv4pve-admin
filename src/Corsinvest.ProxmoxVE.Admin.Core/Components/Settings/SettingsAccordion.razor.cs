/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Components.Settings;

public partial class SettingsAccordion<TSettings>(ISettingsService settingsService,
                                                  NotificationService notificationService,
                                                  IServiceScopeFactory serviceScopeFactory)
{
    [Parameter] public IEnumerable<SettingSection<TSettings>> Sections { get; set; } = [];
    [Parameter] public bool ForCurrentUser { get; set; }

    private TSettings Settings { get; set; } = default!;

    protected override void OnInitialized() => Settings = settingsService.Get<TSettings>(ForCurrentUser);

    private Dictionary<string, object> GetParameters() => new()
    {
        [nameof(ISettingsParameter<>.Settings)] = Settings!,
        [nameof(ISettingsParameter<>.SettingsChanged)] = EventCallback.Factory.Create<TSettings>(this, OnSettingsChanged)
    };

    private void OnSettingsChanged(TSettings settings)
    {
        Settings = settings;
        StateHasChanged();
    }

    private async Task SaveSectionAsync(SettingSection<TSettings> section)
    {
        await settingsService.SetAsync(Settings, ForCurrentUser);
        notificationService.Success(L["Settings saved!"]);

        if (section.OnSavedAsync != null)
        {
            await using var scope = serviceScopeFactory.CreateAsyncScope();
            await section.OnSavedAsync(scope, Settings);
        }
    }
}
