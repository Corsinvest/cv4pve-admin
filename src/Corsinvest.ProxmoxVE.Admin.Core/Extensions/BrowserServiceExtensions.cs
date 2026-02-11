namespace Corsinvest.ProxmoxVE.Admin.Core.Extensions;

public static class BrowserServiceExtensions
{
    public static Task OpenPveConsole(this IBrowserService browserService, string url)
        => browserService.OpenInNewWindowAsync(url, "toolbar=no,location=no,status=no,menubar=no,resizable=yes,width=800,height=420");

}
