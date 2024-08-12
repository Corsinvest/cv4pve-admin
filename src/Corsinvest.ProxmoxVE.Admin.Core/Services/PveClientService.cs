/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Blazored.LocalStorage;
using Corsinvest.ProxmoxVE.Admin.Core.Services.DiskInfo;
using Corsinvest.ProxmoxVE.Api;
using Corsinvest.ProxmoxVE.Api.Extension.Utils;
using Corsinvest.ProxmoxVE.Api.Shared;
using Corsinvest.ProxmoxVE.Api.Shared.Utils;

namespace Corsinvest.ProxmoxVE.Admin.Core.Services;

public class PveClientService : IPveClientService
{
    //private PveClient _client = default!;
    private readonly IOptionsSnapshot<AdminOptions> _clustersOptions;
    private readonly ILocalStorageService _localStorageService;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<PveClientService> _logger;

    public PveClientService(IOptionsSnapshot<AdminOptions> clustersOptions,
                            ILocalStorageService localStorageService,
                            ILoggerFactory loggerFactory,
                            ILogger<PveClientService> logger)
    {
        _clustersOptions = clustersOptions;
        _localStorageService = localStorageService;
        _loggerFactory = loggerFactory;
        _logger = logger;
    }

    public async Task<PveClient?> GetClientAsync(ClusterOptions clusterOptions)
    {
        var client = await GetClientAsync(clusterOptions, _logger);
        if (client != null) { client.LoggerFactory = _loggerFactory; }
        return client;
    }

    public async Task<PveClient?> GetClientAsync(ClusterOptions clusterOptions, ILogger logger)
    {
        try
        {
            var client = ClientHelper.GetClientFromHA(clusterOptions.ApiHostsAndPortHA, clusterOptions.Timeout);
            client.ValidateCertificate = clusterOptions.VerifyCertificate;
            if (client != null)
            {
                bool login;
                if (clusterOptions.UseApiToken)
                {
                    client.ApiToken = clusterOptions.ApiToken;
                    login = (await client.Version.Version()).IsSuccessStatusCode;
                }
                else
                {
                    login = await client.LoginAsync(clusterOptions.ApiCredential.Username, clusterOptions.ApiCredential.Password);
                }

                if (!login) { logger.LogError("GetPveClient error! {error}", client.LastResult.ReasonPhrase); }

                return login
                        ? client
                        : throw new PveException("GetPveClient error! " + client.LastResult.ReasonPhrase);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, nameof(GetClientAsync));
            throw new PveException(ex.Message, ex);
        }

        throw new PveException("GetPveClient error!");
        //return null;
    }

    public async Task<PveClient?> GetClientAsync(string clusterName)
    {
        var clusterOptions = GetClusterOptions(clusterName);
        return clusterOptions == null
                ? null
                : await GetClientAsync(clusterOptions);
    }

    public async Task<PveClient> GetClientCurrentClusterAsync() => (await GetClientAsync(await GetCurrentClusterNameAsync()))!;
    public ClusterOptions? GetClusterOptions(string clusterName) => _clustersOptions.Value.Clusters.FirstOrDefault(a => a.Name == clusterName);
    public async Task<ClusterOptions?> GetCurrentClusterOptionsAsync() => GetClusterOptions(await GetCurrentClusterNameAsync());

    public IEnumerable<ClusterOptions> GetClusters()
        => _clustersOptions.Value.Clusters
                                 .Where(a => !string.IsNullOrEmpty(a.Name))
                                 .OrderBy(a => a.FullName)
                                 .ToList();

    public async Task SetCurrentClusterNameAsync(string clusterName) => await _localStorageService.SetItemAsStringAsync("CurrentClusterName", clusterName);
    public async Task<string> GetCurrentClusterNameAsync() => await _localStorageService.GetItemAsStringAsync("CurrentClusterName");

    public async Task<int> PopulateInfoNodesAsync(ClusterOptions clusterOptions)
    {
        var ret = 0;
        var client = await GetClientAsync(clusterOptions);
        if (client != null)
        {
            var info = await client.GetClusterInfoAsync();
            clusterOptions.Name = info.Name;
            clusterOptions.Type = info.Type;

            var status = await client.Cluster.Status.GetAsync();

            //check new nodes
            foreach (var item in status.Where(a => !string.IsNullOrWhiteSpace(a.IpAddress)))
            {
                if (clusterOptions.GetNodeOptions(item.IpAddress, item.Name) == null)
                {
                    clusterOptions.Nodes.Add(new()
                    {
                        IpAddress = item.IpAddress
                    });
                    ret = 1;
                }
            }

            //get server id
            foreach (var node in clusterOptions.Nodes)
            {
                var nodeStatus = status.FirstOrDefault(x => x.IpAddress == node.IpAddress);
                if (nodeStatus != null && nodeStatus.IsOnline)
                {
                    var serverId = (await client.Nodes[nodeStatus.Name].Subscription.GetAsync()).Serverid;
                    if (node.ServerId != serverId)
                    {
                        node.ServerId = serverId;
                        ret = 1;
                    }
                }
            }
        }
        else
        {
            ret = -1;
        }

        return ret;
    }


    public async Task<bool> ClusterIsValidAsync(string clusterName)
    {
        try
        {
            var ret = false;
            if (!string.IsNullOrEmpty(clusterName))
            {
                var client = await GetClientAsync(clusterName);
                ret = client != null && await CheckIsValidVersionAsync(client);
            }
            return ret;
        }
        catch // (Exception ex)
        {
            return false;
        }
    }

    public async Task<bool> ExistsCurrentClusterNameAsync()
    {
        var currentClusterName = await GetCurrentClusterNameAsync();
        return _clustersOptions.Value.Clusters.Any(a => a.Name == currentClusterName);
    }

    public async Task<bool> CheckIsValidVersionAsync(PveClient client)
    {
        var info = await client.Version.GetAsync();
        return Version.TryParse(info.Version.Split("-")[0], out var version)
                && version >= PveAdminHelper.MinimalVersion;
    }

    public async Task<Api.Shared.Models.Cluster.ClusterStatus?> GetClusterStatusAsync(PveClient client)
        => (await client.Cluster.Status.GetAsync()).FirstOrDefault(a => a.Type == PveConstants.KeyApiCluster);

    public string GetUrl(ClusterOptions clusterOptions)
    {
        var client = ClientHelper.GetClientFromHA(clusterOptions.ApiHostsAndPortHA, 1000);
        return client == null
            ? throw new PveException("GetUrl error")
            : $"https://{client.Host}:{client.Port}";
    }

    public async Task<IEnumerable<DiskInfoBase>> GetDisksInfoAsync(PveClient client, ClusterOptions clusterOptions)
    {
        var ret = new List<DiskInfoBase>();

        if (clusterOptions.CalculateSnapshotSize)
        {
            ret.AddRange(await ZfsDiskInfo.ReadAsync(client, clusterOptions));
            ret.AddRange(await CephDiskInfo.ReadAsync(client, clusterOptions));
        }

        return ret;
    }
}