/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Blazored.LocalStorage;
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

    public async Task<PveClient?> GetClient(ClusterOptions clusterOptions)
    {
        var client = await GetClient(clusterOptions, _logger);
        client.LoggerFactory = _loggerFactory;
        return client;
    }

    public async Task<PveClient?> GetClient(ClusterOptions clusterOptions, ILogger logger)
    {
        try
        {
            var client = ClientHelper.GetClientFromHA(clusterOptions.ApiHostsAndPortHA, clusterOptions.Timeout);
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
                    login = await client.Login(clusterOptions.ApiCredential.Username, clusterOptions.ApiCredential.Password);
                }

                if (!login) { logger.LogError("GetPveClient error! {error}", client.LastResult.ReasonPhrase); }

                return login
                        ? client
                        : throw new PveException("GetPveClient error! " + client.LastResult.ReasonPhrase);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, nameof(GetClient));
            throw new PveException(ex.Message, ex);
        }

        throw new PveException("GetPveClient error!");
        //return null;
    }

    public async Task<PveClient?> GetClient(string clusterName)
    {
        var clusterOptions = GetClusterOptions(clusterName);
        return clusterOptions == null
                ? null
                : await GetClient(clusterOptions);
    }

    public async Task<PveClient> GetClientCurrentCluster() => (await GetClient(await GetCurrentClusterName()))!;
    public ClusterOptions? GetClusterOptions(string clusterName) => _clustersOptions.Value.Clusters.FirstOrDefault(a => a.Name == clusterName);
    public async Task<ClusterOptions?> GetCurrentClusterOptions() => GetClusterOptions(await GetCurrentClusterName());

    public IEnumerable<ClusterOptions> GetClusters()
        => _clustersOptions.Value.Clusters
                                 .Where(a => !string.IsNullOrEmpty(a.Name))
                                 .OrderBy(a => a.FullName)
                                 .ToList();

    public async Task SetCurrentClusterName(string clusterName) => await _localStorageService.SetItemAsStringAsync("CurrentClusterName", clusterName);
    public async Task<string> GetCurrentClusterName() => await _localStorageService.GetItemAsStringAsync("CurrentClusterName");

    public async Task<int> PopulateInfoNodes(ClusterOptions clusterOptions)
    {
        var ret = 0;
        var client = await GetClient(clusterOptions);
        if (client != null)
        {
            var status = await client.Cluster.Status.Get();

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
                    var serverId = (await client.Nodes[nodeStatus.Name].Subscription.GetEx()).Serverid;
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


    public async Task<bool> ClusterIsValid(string clusterName)
    {
        try
        {
            var ret = false;
            if (!string.IsNullOrEmpty(clusterName))
            {
                var client = await GetClient(clusterName);
                ret = client != null && await CheckIsValidVersion(client);
            }
            return ret;
        }
        catch // (Exception ex)
        {
            return false;
        }
    }

    public async Task<bool> ExistsCurrentClusterName()
    {
        var currentClusterName = await GetCurrentClusterName();
        return _clustersOptions.Value.Clusters.Any(a => a.Name == currentClusterName);
    }

    public async Task<bool> CheckIsValidVersion(PveClient client)
    {
        var info = await client.Version.Get();
        return Version.TryParse(info.Version.Split("-")[0], out var version)
                && version >= PveAdminHelper.MinimalVersion;
    }

    public async Task<Api.Shared.Models.Cluster.ClusterStatus?> GetClusterStatus(PveClient client)
        => (await client.Cluster.Status.Get()).FirstOrDefault(a => a.Type == PveConstants.KeyApiCluster);

    public string GetUrl(ClusterOptions clusterOptions)
    {
        var client = ClientHelper.GetClientFromHA(clusterOptions.ApiHostsAndPortHA, 1000);
        return client == null
            ? throw new PveException("GetUrl error")
            : $"https://{client.Host}:{client.Port}";
    }
}