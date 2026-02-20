/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Microsoft.AspNetCore.Components.Routing;
using Toolbelt.Blazor.HotKeys2;
using Wangkanai.Detection.Services;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components.Layout;

public partial class HelpMenu(ISettingsService settingsService,
                              IReleaseService releaseService,
                              DialogService dialogService,
                              ContextMenuService contextMenuService,
                              Services.IBrowserService browserService,
                              IAdminService adminService,
                              IDetectionService detectionService,
                              NavigationManager navigationManager,
                              IModuleService moduleService) : IDisposable
{
    [Parameter] public HotKeysContext HotKeysContext { get; set; } = default!;

    private AppSettings AppSettings { get; set; } = default!;
    private ReleaseInfo? NewRelease { get; set; }
    private ModuleBase? CurrentModule { get; set; }

    private string DocumentationUrl
        => CurrentModule?.HelpUrl is { Length: > 0 } helpUrl
            ? $"{ApplicationHelper.DocumentationUrl}/{helpUrl}"
            : ApplicationHelper.DocumentationUrl;

    private string DocumentationText
        => CurrentModule?.HelpUrl is { Length: > 0 }
            ? $"{L["Documentation"]} - {CurrentModule.Name}"
            : L["Documentation"];

    protected override async Task OnInitializedAsync()
    {
        AppSettings = settingsService.GetAppSettings();
        NewRelease = await releaseService.NewReleaseIsAvailableAsync(includePrerelease: BuildInfo.IsTesting);
        navigationManager.LocationChanged += OnLocationChanged;
        UpdateCurrentModule(navigationManager.Uri);
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e) => UpdateCurrentModule(e.Location);
    private void UpdateCurrentModule(string path) => CurrentModule = moduleService.GetByUrl(path);
    public void Dispose() => navigationManager.LocationChanged -= OnLocationChanged;

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
