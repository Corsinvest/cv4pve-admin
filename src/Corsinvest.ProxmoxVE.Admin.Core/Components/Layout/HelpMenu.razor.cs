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
    private bool IsUpdating { get; set; }
    private ModuleBase? CurrentModule { get; set; }

    private string DocumentationUrl
        => CurrentModule?.HelpUrl is { Length: > 0 } helpUrl
            ? $"{ApplicationHelper.DocumentationUrl}/{helpUrl}"
            : ApplicationHelper.DocumentationUrl;

    private string DocumentationText
        => CurrentModule?.HelpUrl is { Length: > 0 }
            ? L["Documentation - {0}", CurrentModule.Name]
            : L["Documentation"];

    protected override async Task OnInitializedAsync()
    {
        AppSettings = settingsService.GetAppSettings();
        NewRelease = await releaseService.NewReleaseIsAvailableAsync(includePrerelease: BuildInfo.IsTesting);
        releaseService.NewReleaseDiscovered += OnNewReleaseDiscovered;
        navigationManager.LocationChanged += OnLocationChanged;
        UpdateCurrentModule(navigationManager.Uri);
    }

    private void OnNewReleaseDiscovered(object? sender, ReleaseInfo releaseInfo)
    {
        NewRelease = releaseInfo;
        InvokeAsync(StateHasChanged);
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e) => UpdateCurrentModule(e.Location);
    private void UpdateCurrentModule(string path) => CurrentModule = moduleService.GetByUrl(path);

    public void Dispose()
    {
        releaseService.NewReleaseDiscovered -= OnNewReleaseDiscovered;
        navigationManager.LocationChanged -= OnLocationChanged;
    }

    private async Task OpenWhoIsUsingUrlAsync()
    {
        var whoBodyBuilder = new System.Text.StringBuilder();
        foreach (var item in adminService.Where(a => a.Settings.Enabled))
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
        foreach (var item in adminService.Where(a => a.Settings.Enabled))
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

        envBuilder.AppendLine($"- Deployment: {(BuildInfo.IsInContainer ? "Docker" : "Native")}");
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

    private async Task TriggerUpdateAsync()
    {
        var confirmed = await dialogService.ConfirmAsync(
            L["Are you sure you want to trigger the automatic update? The application will be updated to version {0}.", NewRelease?.Version ?? ""],
            L["Confirm Update"],
            true,
            L["Yes, Update"],
            L["Cancel"]);

        if (!confirmed) { return; }

        try
        {
            IsUpdating = true;
            await InvokeAsync(StateHasChanged);

            var success = await releaseService.TriggerUpdateAsync();

            if (success)
            {
                await dialogService.Alert(L["Update to version {0} has been triggered successfully. The application will be updated automatically in a few moments.", NewRelease?.Version ?? ""],
                                          L["Update Started"],
                                          new AlertOptions
                                          {
                                              OkButtonText = L["OK"]
                                          });
            }
            else
            {
                await dialogService.Alert(L["Automatic updates are not supported in this environment. Please download the latest version manually from {0}.", NewRelease?.Url ?? ""],
                                          L["Update Not Supported"],
                                          new AlertOptions
                                          {
                                              OkButtonText = L["OK"]
                                          });
            }
        }
        finally
        {
            IsUpdating = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task ShowReleaseNotesAsync()
        => await dialogService.OpenSideExAsync<ReleaseNotesDialog>(L["Release Notes"],
                                                                   [],
                                                                   new DialogOptions
                                                                   {
                                                                       CloseDialogOnOverlayClick = true,
                                                                   });
}
