/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Blazored.LocalStorage;
using ClosedXML.Excel;
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

    public async Task<bool> IsValidCurrentClusterName()
    {
        var currentClusterName = await GetCurrentClusterName();
        return _clustersOptions.Value.Clusters.Any(a => a.Name == currentClusterName);
    }
}