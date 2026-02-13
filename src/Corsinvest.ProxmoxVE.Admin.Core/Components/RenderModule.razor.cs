/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Components;

public partial class RenderModule(NavigationManager navigationManager,
                                  IModuleService moduleService,
                                  IDialogServiceEx dialogServiceEx,
                                  ISettingsService settingsService) : IClusterName
{
    [CascadingParameter(Name = nameof(ClusterName))] public string ClusterName { get; set; } = default!;
    [Parameter] public string? PageRoute { get; set; }

    public string BodyStyle => "padding-top: 0; padding-bottom: 0; padding-right:0;"
                                + (NavBar.Any() ? string.Empty : "padding-left: 0");

    //private AppSettings AppSettings { get; set; } = default!;
    private ModuleBase CurrentModule { get; set; } = default!;
    private RenderComponentInfo? Render { get; set; }
    private IEnumerable<ModuleLinkBase> NavBar { get; set; } = [];
    private bool ShowButtonSettings { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        //AppSettings = SettingsService.GetAppSettings();
        var routes = (PageRoute ?? string.Empty).Split('/');

        CurrentModule = moduleService.GetBySlug(routes[0])!;
        if (CurrentModule == null)
        {
            navigationManager.NavigateTo("/NotFound", false);
            return;
        }

        //permission
        if (!await CurrentModule.HasPermissionLinkAsync(PermissionService, ClusterName))
        {
            navigationManager.NavigateTo("/NotAuthorized", false);
            return;
        }

        ShowButtonSettings = CurrentModule.RenderSettingsInfo != null
                                && await CurrentModule.HasPermissionEditorSettingsAsync(PermissionService, ClusterName);

        //parse link
        var subItems = routes.Skip(1).ToList();

        if (CurrentModule.Scope == ClusterScope.Single)
        {
            ////specify clusterName in url
            //ClusterName = subItems[0];

            //not exists
            if (string.IsNullOrEmpty(ClusterName))
            {
                navigationManager.NavigateTo("/NotFound", false);
                return;
            }

            //exists
            var clusterSettings = settingsService.GetClusterSettings(ClusterName);
            if (clusterSettings is null || !clusterSettings!.Enabled)
            {
                navigationManager.NavigateTo("/NotFound", false);
                return;
            }

            // subItems = subItems.Skip(1).ToList();
        }

        var url = subItems.JoinAsString("/");

        var link = CurrentModule.NavBar.Any()
                    ? CurrentModule.NavBar.Traverse(a => a.Child).FirstOrDefault(a => a.Url == url)
                    : CurrentModule.Link;

        if (link == null)
        {
            navigationManager.NavigateTo("/NotFound", false);
            return;
        }

        if (!await link?.HasPermissionAsync(PermissionService, ClusterName)!)
        {
            navigationManager.NavigateTo("/NotAuthorized", false);
            return;
        }

        Render = link?.Render;

        NavBar = await MakeNavBar(CurrentModule.NavBar);
    }

    private async Task<IEnumerable<ModuleLinkBase>> MakeNavBar(IEnumerable<ModuleLinkBase> items)
    {
        var ret = new List<ModuleLinkBase>();
        foreach (var item in items)
        {
            if (await item?.HasPermissionAsync(PermissionService, ClusterName)!)
            {
                if (item.Child.Any())
                {
                    //var retSub = await MakeNavBar(item.Child);
                    ret.Add(item);
                }
                else
                {
                    ret.Add(item);
                }
            }
        }
        return ret;
    }

    private async Task OpenSettingsAsync()
        => await dialogServiceEx.OpenSettingsAsync(CurrentModule, CurrentModule.GetClusterNameForScope(ClusterName));
}
