using Corsinvest.ProxmoxVE.Admin.Core.Helpers;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;

namespace Corsinvest.ProxmoxVE.Admin.Components;

public partial class App(ThemeService themeService, NavigationManager navigationManager)
{
    [CascadingParameter] private HttpContext HttpContext { get; set; } = default!;

    //private static IComponentRenderMode InteractiveServerWithoutPrerendering = new InteractiveServerRenderMode(prerender: false);

    //private IComponentRenderMode? RenderModeForPage
    //    => HttpContext.Request.Path.StartsWithSegments("/Account")
    //        ? null
    //        : RenderMode.InteractiveServer;

    private IComponentRenderMode? PageRenderMode
        => HttpContext.AcceptsInteractiveRouting() ? new InteractiveServerRenderMode(prerender: false) : null;

    protected override void OnInitialized()
    {
        base.OnInitialized();

        if (HttpContext != null)
        {
            var theme = HttpContext.Request.Cookies[ApplicationHelper.CookieThemeName];
            if (!string.IsNullOrEmpty(theme)) { themeService.SetTheme(theme, false); }
        }
    }
}
