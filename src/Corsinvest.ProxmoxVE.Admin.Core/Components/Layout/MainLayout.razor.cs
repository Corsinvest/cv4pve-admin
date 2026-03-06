/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth.Permissions;
using Corsinvest.ProxmoxVE.Admin.Core.Security.Identity;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Identity;
using Toolbelt.Blazor.HotKeys2;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components.Layout;

public partial class MainLayout : IDisposable, IAsyncDisposable
{
    [Inject] private IModuleService ModuleService { get; set; } = default!;
    [Inject] private IPermissionService PermissionService { get; set; } = default!;
    [Inject] private IStringLocalizer<MainLayout> L { get; set; } = default!;
    [Inject] private ICurrentClusterService CurrentClusterService { get; set; } = default!;
    [Inject] private ISettingsService SettingsService { get; set; } = default!;
    [Inject] private ICurrentUserService CurrentUserService { get; set; } = default!;
    [Inject] private UserManager<ApplicationUser> UserManager { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private HotKeys HotKeys { get; set; } = default!;
    [Inject] private NavigationTrackerService NavigationTrackerService { get; set; } = default!;

    private bool SidebarExpanded { get; set; } = true;
    private bool HasRendered { get; set; }
    private IEnumerable<ModuleLinkBase> NavBarLinks { get; set; } = [];
    private IEnumerable<ModuleLinkBase> ProfileMenuLinks { get; set; } = [];
    private IEnumerable<ModuleLinkBase> HeaderLinks { get; set; } = [];
    private string ClusterName { get; set; } = default!;
    private ErrorBoundary ErrorBoundaryRef { get; set; } = default!;
    protected virtual Type[] AdditionalLayoutComponents => [];
    private AppSettings AppSettings { get; set; } = default!;
    private CommandPalette? CommandPaletteRef { get; set; }
    private HotKeysContext? HotKeysContext { get; set; }

    protected override void OnParametersSet() => ErrorBoundaryRef?.Recover();

    protected override async Task OnInitializedAsync()
    {
        AppSettings = SettingsService.GetAppSettings();

        if (await CurrentUserService.GetUserAsync() == null)
        {
            NavigationManager.NavigateTo("/Logout", true);
            return;
        }

        HotKeysContext = HotKeys.CreateContext()
            .Add(ModCode.Ctrl, Code.K, OpenCommandPalette, new HotKeyOptions { Description = "Open Command Palette" });
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            NavigationManager.LocationChanged += OnLocationChanged;

            ClusterName = UrlHelper.GetClusterNameFromUrl(NavigationManager.Uri)
                          ?? ApplicationHelper.AllClusterName;
            CurrentClusterService.ClusterName = ClusterName;

            var user = await CurrentUserService.GetUserAsync();
            if (user != null && await UserManager.CheckPasswordAsync(user, ApplicationHelper.DefaultAdminPassword))
            {
                NotifyDefaultPassword();
            }

            await RefreshDataAsync();
            HasRendered = true;
            await InvokeAsync(StateHasChanged);
        }
    }

    private bool HasNoClusters { get; set; }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        var clusterFromUrl = UrlHelper.GetClusterNameFromUrl(e.Location);
        if (clusterFromUrl != null && !ApplicationHelper.IsAllCluster(clusterFromUrl) && clusterFromUrl != ClusterName)
        {
            ClusterName = clusterFromUrl;
            CurrentClusterService.ClusterName = ClusterName;
            _ = InvokeAsync(async () => await RefreshDataAsync());
        }
    }

    private void CheckExistCluster()
    {
        if (!SettingsService.GetEnabledClustersSettings().Any())
        {
            NavigationManager.NavigateTo(UrlHelper.UrlNewPveConfig);
        }
    }

    private async Task OpenCommandPalette()
    {
        if (CommandPaletteRef != null) { await CommandPaletteRef.Open(); }
    }

    private async Task SetClusterNameAsync()
    {
        CurrentClusterService.ClusterName = ClusterName;
        var currentModule = ModuleService.GetByUrl(NavigationManager.Uri);
        if (currentModule != null)
        {
            NavigationManager.NavigateTo(currentModule.Link!.GetRealUrl(ClusterName));
        }

        await RefreshDataAsync();
    }

    private async Task RefreshDataAsync()
    {
        CheckExistCluster();
        await RefreshLinksAsync();
        await InvokeAsync(StateHasChanged);
    }

    private async Task RefreshLinksAsync()
    {
        var links = ModuleService.Modules
                                 .Where(a => a.ModuleType == ModuleType.Application && a.Link?.Enabled == true)
                                 .Select(a => a.Link!)
                                 .OrderBy(a => a.OrderIndex)
                                 .ThenBy(a => a.Text)
                                 .ToList();

        var existsSettings = SettingsService.GetEnabledClustersSettings().Any();
        var selectedCluster = ApplicationHelper.IsAllCluster(ClusterName);

        foreach (var item in links.ToArray())
        {
            if (!await item.HasPermissionAsync(PermissionService, ClusterName))
            {
                links.Remove(item);
            }
            else if (selectedCluster && item.Module.Scope == ClusterScope.Single)
            {
                links.Remove(item);
            }
            //else if (item.Module.LinkPosition == ModuleLinkPosition.ProfileMenu)
            //{
            //}
            //else if(!existsSettings)
            //{
            //    links.Remove(item);
            //}

            //if (!await item.HasPermissionAsync(permissionService, ClusterName)
            //    || (selectedCluster && item.Module.Scope == ClusterScope.Single)
            //    || !existsSettings)
            //{
            //    links.Remove(item);
            //}
        }

        NavBarLinks = links.Where(a => a.Module.LinkPosition == ModuleLinkPosition.NavBar);
        HeaderLinks = links.Where(a => a.Module.LinkPosition == ModuleLinkPosition.MainHeader);
        ProfileMenuLinks = links.Where(a => a.Module.LinkPosition == ModuleLinkPosition.ProfileMenu);
    }

    public void Dispose()
    {
        NavigationManager.LocationChanged -= OnLocationChanged;
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        NavigationTrackerService.Dispose();
        Dispose();
        if (HotKeysContext != null) { await HotKeysContext.DisposeAsync(); }
    }
}
