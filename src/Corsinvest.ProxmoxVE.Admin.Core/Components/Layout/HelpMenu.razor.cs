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
                              Services.IBrowserService browserService,
                              IAdminService adminService,
                              IDetectionService detectionService)
{
    [Parameter] public HotKeysContext HotKeysContext { get; set; } = default!;

    private AppSettings AppSettings { get; set; } = default!;
    private ReleaseInfo? NewRelease { get; set; }

    protected override async Task OnInitializedAsync()
    {
        AppSettings = settingsService.GetAppSettings();
        NewRelease = await releaseService.NewReleaseIsAvailableAsync(includePrerelease: BuildInfo.IsTesting);
    }

    private async Task OnMenuClick(RadzenProfileMenuItem item)
    {
        switch (item.Value?.ToString())
        {
            case "shortcuts": await ShowKeyboardShortcutsAsync(); break;
            case "release-notes": await ShowReleaseNotesAsync(); break;
            case "report-bug": await OpenBugUrlAsync(); break;
            case "who-is-using": await OpenWhoIsUsingUrlAsync(); break;
            default: break;
        }
    }

    private async Task<IEnumerable<ClusterClient>> GetClustersAsync()
    {
        var clusterName = await adminService.GetCurrentClusterNameAsync();
        return adminService.Where(a => a.Settings.Enabled)
                           .Where(a => a.Settings.Name == clusterName, !string.IsNullOrEmpty(clusterName));
    }

    private async Task OpenWhoIsUsingUrlAsync()
    {
        var whoBodyBuilder = new System.Text.StringBuilder();
        foreach (var item in await GetClustersAsync())
        {
            try
            {
                if (whoBodyBuilder.Length > 0) { whoBodyBuilder.AppendLine("---"); }
                whoBodyBuilder.AppendLine(await PveAdminHelper.GenerateWhoUsingAsync(item));
            }
            catch { }
        }

        var url = ApplicationHelper.GetWhoIsUsingUrl(whoBodyBuilder.ToString().TrimEnd());
        await browserService.OpenInNewWindowAsync(url, string.Empty);
    }

    private async Task OpenBugUrlAsync()
    {
        var envBuilder = new System.Text.StringBuilder();
        foreach (var item in await GetClustersAsync())
        {
            try
            {
                var client = await item.GetPveClientAsync();
                envBuilder.AppendLine($"- Cluster: {item.Settings.Name} / Proxmox VE: {(await client.Version.GetAsync()).Version}");
            }
            catch
            {
                envBuilder.AppendLine($"- Cluster: {item.Settings.Name}");
            }
        }

        envBuilder.AppendLine($"- Deployment: {(ApplicationHelper.IsInContainer ? "Docker" : "Native")}");
        envBuilder.AppendLine($"- Browser: {detectionService.Browser.Name} {detectionService.Browser.Version}");
        envBuilder.AppendLine($"- Platform: {detectionService.Platform.Name} {detectionService.Platform.Version}");

        var url = ApplicationHelper.GetBugReportUrl(envBuilder.ToString().TrimEnd());
        await browserService.OpenInNewWindowAsync(url, string.Empty);
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
