/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Blazored.LocalStorage;
using Corsinvest.ProxmoxVE.Api;

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
        var client = await PveAdminHelper.GetPveClient(clusterOptions, _logger);
        client.LoggerFactory = _loggerFactory;
        return client;
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
            foreach (var item in status)
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
                if (nodeStatus != null)
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
                ret = client != null && await PveAdminHelper.CheckIsValidVersion(client);
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
}