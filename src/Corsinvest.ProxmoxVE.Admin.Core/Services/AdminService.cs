/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Collections;
using Corsinvest.ProxmoxVE.Admin.Core.Clients.Pve;

namespace Corsinvest.ProxmoxVE.Admin.Core.Services;

internal class AdminService(IPveClientFactory pveClientFactory,
                            ISettingsService settingsService,
                            IServiceProvider serviceProvider,
                            IFusionCache fusionCache) : IAdminService
{
    private IEnumerator<ClusterClient> GetEnumeratorInt()
    {
        foreach (var item in settingsService.GetEnabledClustersSettings())
        {
            yield return CreateClusterClient(item);
        }
    }

    public ClusterClient CreateClusterClient(ClusterSettings clusterSettings) => new(pveClientFactory, clusterSettings, fusionCache, serviceProvider);

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

    public async Task<FluentResults.Result<string>> TestSshAsync(ClusterSettings clusterSettings)
    {
        var clusterClient = Exists(clusterSettings.Name)
                               ? this[clusterSettings.Name]
                               : CreateClusterClient(clusterSettings);

        return await clusterClient.TestSshAsync();
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
