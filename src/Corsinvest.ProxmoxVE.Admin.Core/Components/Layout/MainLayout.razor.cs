/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth.Permissions;
using Corsinvest.ProxmoxVE.Admin.Core.Security.Identity;
using Toolbelt.Blazor.HotKeys2;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components.Layout;

public partial class MainLayout(IModuleService moduleService,
                                IPermissionService permissionService,
                                IStringLocalizer<MainLayout> L,
                                IAdminService adminService,
                                ICurrentClusterService currentClusterService,
                                ISettingsService settingsService,
                                ICurrentUserService currentUserService,
                                NavigationManager navigationManager,
                                IReleaseService releaseService,
                                DialogService dialogService,
                                HotKeys hotKeys) : IDisposable, IAsyncDisposable
{
    private bool SidebarExpanded { get; set; } = true;
    private bool HasRendered { get; set; }
    private IEnumerable<ModuleLinkBase> NavBarLinks { get; set; } = [];
    private IEnumerable<ModuleLinkBase> ProfileMenuLinks { get; set; } = [];
    private IEnumerable<ModuleLinkBase> HeaderLinks { get; set; } = [];
    private string ClusterName { get; set; } = default!;
    private ErrorBoundary ErrorBoundaryRef { get; set; } = default!;
    private ReleaseInfo? NewRelease { get; set; }
    private bool IsUpdating { get; set; }
    protected virtual Type[] AdditionalLayoutComponents => [];
    private AppSettings AppSettings { get; set; } = default!;
    private CommandPalette? CommandPaletteRef { get; set; }
    private HotKeysContext? HotKeysContext { get; set; }

    protected override void OnParametersSet() => ErrorBoundaryRef?.Recover();

    protected override async Task OnInitializedAsync()
    {
        AppSettings = settingsService.GetAppSettings();

        if (await currentUserService.GetUserAsync() == null)
        {
            navigationManager.NavigateTo("/Logout", true);
        }

        // Subscribe to new release notifications
        releaseService.NewReleaseDiscovered += OnNewReleaseDiscovered;

        HotKeysContext = hotKeys.CreateContext()
            .Add(ModCode.Ctrl, Code.K, OpenCommandPalette, new HotKeyOptions { Description = "Open Command Palette" });
    }

    private void OnNewReleaseDiscovered(object? sender, ReleaseInfo releaseInfo)
    {
        NewRelease = releaseInfo;
        InvokeAsync(StateHasChanged);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            ClusterName = await adminService.GetCurrentClusterNameAsync();
            currentClusterService.ClusterName = ClusterName;
            await RefreshDataAsync();
            HasRendered = true;
            await InvokeAsync(StateHasChanged);

            if (!settingsService.GetEnabledClustersSettings().Any())
            {
                navigationManager.NavigateTo(ApplicationHelper.UrlNewPveConfig);
            }
        }
    }

    private async Task OpenCommandPalette()
    {
        if (CommandPaletteRef != null) { await CommandPaletteRef.Open(); }
    }

    private async Task SetClusterNameAsync()
    {
        await adminService.SetCurrentClusterNameAsync(ClusterName);
        currentClusterService.ClusterName = ClusterName;
        await RefreshDataAsync();
    }

    private async Task RefreshDataAsync()
    {
        await RefreshLinksAsync();
        await InvokeAsync(StateHasChanged);
    }

    private async Task RefreshLinksAsync()
    {
        var links = moduleService.Modules
                                 .Where(a => a.ModuleType == ModuleType.Application && a.Link?.Enabled == true)
                                 .Select(a => a.Link!)
                                 .OrderBy(a => a.OrderIndex)
                                 .ThenBy(a => a.Text)
                                 .ToList();

        var existsSettings = settingsService.GetEnabledClustersSettings().Any();
        var selectedCluster = string.IsNullOrEmpty(ClusterName);

        foreach (var item in links.ToArray())
        {
            if (!await item.HasPermissionAsync(permissionService, ClusterName)
                || (selectedCluster && item.Module.Scope == ClusterScope.Single)
                || !existsSettings)
            {
                links.Remove(item);
            }
        }

        NavBarLinks = links.Where(a => a.Module.LinkPosition == ModuleLinkPosition.NavBar);
        HeaderLinks = links.Where(a => a.Module.LinkPosition == ModuleLinkPosition.MainHeader);
        ProfileMenuLinks = links.Where(a => a.Module.LinkPosition == ModuleLinkPosition.ProfileMenu);
    }

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
                await dialogService.Alert(
                    L["Update to version {0} has been triggered successfully. The application will be updated automatically in a few moments.", NewRelease?.Version ?? ""],
                    L["Update Started"],
                    new AlertOptions { OkButtonText = "OK" });
            }
            else
            {
                await dialogService.Alert(
                    L["Failed to trigger update. Please try again or update manually."],
                    L["Update Failed"],
                    new AlertOptions { OkButtonText = "OK" });
            }
        }
        catch (NotSupportedException)
        {
            await dialogService.Alert(
                L["Automatic updates are not supported in this environment. Please download the latest version manually from {0}.", NewRelease?.Url ?? ""],
                L["Update Not Supported"],
                new AlertOptions { OkButtonText = "OK" });
        }
        catch (Exception ex)
        {
            await dialogService.Alert(
                L["An error occurred while triggering the update: {0}", ex.Message],
                L["Error"],
                new AlertOptions { OkButtonText = "OK" });
        }
        finally
        {
            IsUpdating = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    public void Dispose()
    {
        releaseService.NewReleaseDiscovered -= OnNewReleaseDiscovered;
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        Dispose();
        if (HotKeysContext != null) { await HotKeysContext.DisposeAsync(); }
    }
}
