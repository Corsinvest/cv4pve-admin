/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Toolbelt.Blazor.HotKeys2;
using Wangkanai.Detection.Services;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components.Layout;

public partial class HelpMenu(ISettingsService settingsService,
                              IReleaseService releaseService,
                              DialogService dialogService,
                              IAdminService adminService,
                              IDetectionService detectionService)
{
    [Parameter] public HotKeysContext HotKeysContext { get; set; } = default!;

    private AppSettings AppSettings { get; set; } = default!;
    private ReleaseInfo? NewRelease { get; set; }
    private string BugReportUrl { get; set; } = string.Empty;
    private string FeatureRequestUrl => ApplicationHelper.FeatureRequestUrl;
    private string FeedbackUrl => ApplicationHelper.FeedbackUrl;

    protected override async Task OnInitializedAsync()
    {
        AppSettings = settingsService.GetAppSettings();
        NewRelease = await releaseService.NewReleaseIsAvailableAsync(false);
        await BuildUrlsAsync();
    }

    protected override async Task OnParametersSetAsync() => await BuildUrlsAsync();

    private async Task BuildUrlsAsync()
    {
        var envBuilder = new System.Text.StringBuilder();

        var clusterName = await adminService.GetCurrentClusterNameAsync();
        foreach (var item in adminService.Where(a=> a.Settings.Enabled)
                                         .Where(a=> a.Settings.Name == clusterName,!string.IsNullOrEmpty(clusterName)))
        {
            try
            {
                var client = await item.GetPveClientAsync();
                var pveVersion = client != null ? (await client.Version.GetAsync()).Version : "N/A";
                envBuilder.AppendLine($"- Cluster: {item.Settings.Name} / Proxmox VE: {pveVersion}");
            }
            catch
            {
                envBuilder.AppendLine($"- Cluster: {item.Settings.Name}");
            }
        }

        envBuilder.AppendLine($"- Deployment: {(ApplicationHelper.IsInContainer ? "Docker" : "Native")}");
        envBuilder.AppendLine($"- Browser: {detectionService.Browser.Name} {detectionService.Browser.Version}");
        envBuilder.AppendLine($"- Platform: {detectionService.Platform.Name} {detectionService.Platform.Version}");

        BugReportUrl = ApplicationHelper.GetBugReportUrl(envBuilder.ToString().TrimEnd());
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
                                                                  new Dictionary<string, object?>
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
