using System.Collections;
using Blazored.SessionStorage;
using Corsinvest.ProxmoxVE.Admin.Core.Clients.Pve;
using Corsinvest.ProxmoxVE.Admin.Core.Security.Identity;

namespace Corsinvest.ProxmoxVE.Admin.Core.Services;

internal class AdminService(IPveClientFactory pveClientFactory,
                            ISettingsService settingsService,
                            ISessionStorageService sessionStorageService,
                            IServiceProvider serviceProvider,
                            ICurrentUserService currentUserService,
                            IFusionCache fusionCache) : IAdminService
{
    private string CurrentClusterNameKeyCookie => $"cv4pve-admin-current-cluster-{currentUserService.UserId.Replace("-", "")}";
    public async Task<string> GetCurrentClusterNameAsync() => (await sessionStorageService.GetItemAsStringAsync(CurrentClusterNameKeyCookie))!;
    public async Task SetCurrentClusterNameAsync(string clusterName) => await sessionStorageService.SetItemAsStringAsync(CurrentClusterNameKeyCookie, clusterName);

    private IEnumerator<ClusterClient> GetEnumeratorInt()
    {
        foreach (var item in settingsService.GetEnabledClustersSettings())
        {
            yield return CreateClusterClient(item);
        }
    }

    private ClusterClient CreateClusterClient(ClusterSettings clusterSettings) => new(pveClientFactory, clusterSettings, fusionCache, serviceProvider);

    public IEnumerator<ClusterClient> GetEnumerator() => GetEnumeratorInt();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumeratorInt();
    public bool Exists(string clusterName) => settingsService.GetClusterSettings(clusterName) != null;

    public ClusterClient this[string clusterName]
    {
        get
        {
            var clusterSettings = settingsService.GetClusterSettings(clusterName);
            return clusterSettings == null || !clusterSettings!.Enabled
                ? throw new ArgumentException(nameof(clusterName))
                : CreateClusterClient(clusterSettings);
        }
    }

    public async Task<FluentResults.Result<string>> PopulateInfoAsync(ClusterSettings clusterSettings)
    {
        var clusterClient = Exists(clusterSettings.Name)
                               ? this[clusterSettings.Name]
                               : CreateClusterClient(clusterSettings);

        return await clusterClient.PopulateSettingsAsync();
    }

    //public async Task<bool> ClusterIsValidAsync(string clusterName)
    //{
    //    try
    //    {
    //        var ret = false;
    //        if (!string.IsNullOrEmpty(clusterName))
    //        {
    //            var client = await GetClientAsync(clusterName);
    //            ret = client != null && await CheckIsValidVersionAsync(client);
    //        }
    //        return ret;
    //    }
    //    catch // (Exception ex)
    //    {
    //        return false;
    //    }
    //}
}
