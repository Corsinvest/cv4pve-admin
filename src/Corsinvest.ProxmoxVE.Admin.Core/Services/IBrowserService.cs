namespace Corsinvest.ProxmoxVE.Admin.Core.Services;

public interface IBrowserService
{
    Task CopyToClipboardAsync(string text);
    Task OpenAsync(string url, string target);
    Task OpenInNewWindowAsync(string url, string windowFeatures);
}
