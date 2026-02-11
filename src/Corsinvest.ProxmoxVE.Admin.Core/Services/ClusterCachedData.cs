using Corsinvest.ProxmoxVE.Admin.Core.Clients.Pve;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Common;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Node;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;

namespace Corsinvest.ProxmoxVE.Admin.Core.Services;

public class ClusterCachedData
{
    private readonly IPveClientFactory _pveClientFactory;
    private readonly IFusionCache _fusionCache;
    private readonly ClusterSettings _clusterSettings;
    private readonly IServiceProvider _serviceProvider;

    internal ClusterCachedData(IPveClientFactory pveClientFactory,
                               IFusionCache fusionCache,
                               ClusterSettings clusterSettings,
                               IServiceProvider serviceProvider)
    {
        _pveClientFactory = pveClientFactory;
        _fusionCache = fusionCache;
        _clusterSettings = clusterSettings;
        _serviceProvider = serviceProvider;
    }

    private string GetKeyPrefix() => $"Cluster:Data:{_clusterSettings.Name}:";
    private string MakeKey(string key) => $"{GetKeyPrefix()}{key}";

    public async Task<T?> GetOrDefaultAsync<T>(string key, T data) => await _fusionCache.GetOrDefaultAsync(MakeKey(key), data);

    public async Task<T> GetOrSetAsync<T>(string key,
                                          T data,
                                          int seconds,
                                          bool forceReload)
    {
        if (forceReload) { await _fusionCache.RemoveAsync(MakeKey(key)); }
        return await _fusionCache.GetOrSetAsync(MakeKey(key), data, TimeSpan.FromSeconds(seconds), tags: [GetKeyPrefix()]);
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> func, int seconds, bool forceReload)
    {
        if (forceReload) { await _fusionCache.RemoveAsync(MakeKey(key)); }
        return await _fusionCache.GetOrSetAsync(MakeKey(key), async _ => await func.Invoke(), TimeSpan.FromSeconds(seconds), tags: [GetKeyPrefix()]);
    }

    public async Task<PveClient> GetPveClientAsync(bool forceReload = false)
        => await GetOrSetAsync($"PveClient:{_clusterSettings.ApiHostsAndPortHA}",
                               await _pveClientFactory.CreateClientAsync(_clusterSettings, CancellationToken.None),
                               60 * 60,
                               forceReload);

    public async Task<IEnumerable<ClusterResource>> GetResourcesAsync(bool forceReload)
        => await GetOrSetAsync(nameof(GetResourcesAsync),
                               async () => (await (await GetPveClientAsync()).GetResourcesAsync(ClusterResourceType.All)).CalculateHostUsage().ToList(),
                               10,
                               forceReload);

    public async Task<string> GetTagStyleColorMapAsync(bool forceReload)
        => await GetOrSetAsync(nameof(GetTagStyleColorMapAsync),
                               async () => (await (await GetPveClientAsync()).Cluster.Options.GetAsync()).TagStyle?.ColorMap ?? string.Empty,
                               60,
                               forceReload);

    public async Task<IEnumerable<VmDiskInfo>> GetDisksInfoAsync(bool forceReload)
        => await GetOrSetAsync(nameof(GetDisksInfoAsync),
                               async () =>
                               {
                                   return _clusterSettings.AllowCalculateSnapshotSize
                                             ? await _serviceProvider.GetRequiredService<ISnapshotSizeService>().GetAsync(await GetPveClientAsync())
                                             : [];
                               },
                               5 * 60,
                               forceReload);

    public async Task<IEnumerable<NodeRrdData>> GetRrdDataAsync(string node,
                                                                RrdDataTimeFrame rrdDataTimeFrame,
                                                                RrdDataConsolidation rrdDataConsolidation,
                                                                bool forceReload)
        => await GetOrSetAsync($"{nameof(GetRrdDataAsync)}:{node}:{rrdDataTimeFrame}:{rrdDataConsolidation}",
                               async () => await (await GetPveClientAsync()).Nodes[node].Rrddata.GetAsync(rrdDataTimeFrame, rrdDataConsolidation),
                               30,
                               forceReload);

    public async Task<IEnumerable<NodeStorageRrdData>> GetRrdDataAsync(string node,
                                                                       string storage,
                                                                       RrdDataTimeFrame rrdDataTimeFrame,
                                                                       RrdDataConsolidation rrdDataConsolidation,
                                                                       bool forceReload)
    => await GetOrSetAsync($"{nameof(GetRrdDataAsync)}:{node}:{storage}:{rrdDataTimeFrame}:{rrdDataConsolidation}",
                           async () => await (await GetPveClientAsync()).Nodes[node].Storage[storage].Rrddata.GetAsync(rrdDataTimeFrame, rrdDataConsolidation),
                           30,
                           forceReload);

    private class DummyClusterResourceVmOsInfo : IClusterResourceVmOsInfo
    {
        public VmQemuAgentOsInfo VmQemuAgentOsInfo { get; set; } = default!;
        public string HostName { get; set; } = default!;
        public string OsVersion { get; set; } = default!;
        public VmOsType? OsType { get; set; }
    }

    public async Task<IClusterResourceVmOsInfo> GetVmOsInfoAsync<T>(T item, bool forceReload)
        where T : IClusterResourceVm, IClusterResourceVmOsInfo
        => await GetOrSetAsync($"{nameof(GetVmOsInfoAsync)}:{item.VmId}",
                               async () =>
                               {
                                   await VmHelper.PopulateVmOsInfoAsync(await GetPveClientAsync(), item);
                                   return new DummyClusterResourceVmOsInfo
                                   {
                                       HostName = item.HostName,
                                       OsType = item.OsType,
                                       OsVersion = item.OsVersion,
                                       VmQemuAgentOsInfo = item.VmQemuAgentOsInfo
                                   };
                               },
                               60 * 30,
                               forceReload);

    public async Task<IEnumerable<VmRrdData>> GetRrdDataAsync(string node,
                                                              VmType vmType,
                                                              long vmId,
                                                              RrdDataTimeFrame rrdDataTimeFrame,
                                                              RrdDataConsolidation rrdDataConsolidation,
                                                              bool forceReload)
        => await GetOrSetAsync($"{nameof(GetRrdDataAsync)}:{node}:{vmType}:{vmId}:{rrdDataTimeFrame}:{rrdDataConsolidation}",
                               async () => await (await GetPveClientAsync()).GetVmRrdDataAsync(node, vmType, vmId, rrdDataTimeFrame, rrdDataConsolidation),
                               30,
                               forceReload);

    public async Task<IEnumerable<VmSnapshot>> GetSnapshotsAsync(string node,
                                                                 VmType vmType,
                                                                 long vmId,
                                                                 bool forceReload)
        => await GetOrSetAsync($"{nameof(GetSnapshotsAsync)}:{node}:{vmType}:{vmId}",
                               async () => await SnapshotHelper.GetSnapshotsAsync(await GetPveClientAsync(), node, vmType, vmId),
                               10,
                               forceReload);

    public async Task<IEnumerable<NodeReplication>> GetReplicationsAsync(string node,
                                                                         long? vmId,
                                                                         bool forceReload)
        => await GetOrSetAsync($"{nameof(GetReplicationsAsync)}:{node}:{vmId ?? 0}",
                               async () => await (await GetPveClientAsync()).Nodes[node].Replication.GetAsync(guest: vmId.HasValue
                                                                                          ? Convert.ToInt32(vmId)
                                                                                          : null),
                               10,
                               forceReload);

    public async Task<IEnumerable<ClusterReplication>> GetClusterReplicationsAsync(bool forceReload)
        => await GetOrSetAsync(nameof(GetClusterReplicationsAsync),
                               async () => await (await GetPveClientAsync()).Cluster.Replication.GetAsync(),
                               60,
                               forceReload);

    public async Task<VmConfigQemu> GetQemuConfigAsync(string node, long vmId, bool forceReload)
        => await GetOrSetAsync($"{nameof(GetQemuConfigAsync)}:{node}:{vmId}",
                               async () => await (await GetPveClientAsync()).Nodes[node].Qemu[vmId].Config.GetAsync(),
                               60,
                               forceReload);

    public async Task<VmConfigLxc> GetLxcConfigAsync(string node, long vmId, bool forceReload)
        => await GetOrSetAsync($"{nameof(GetLxcConfigAsync)}:{node}:{vmId}",
                               async () => await (await GetPveClientAsync()).Nodes[node].Lxc[vmId].Config.GetAsync(),
                               60,
                               forceReload);

    public async Task<IEnumerable<NodeStorageContent>> GetStorageContentsAsync(string node, string storage, bool forceReload)
        => await GetOrSetAsync($"{nameof(GetStorageContentsAsync)}:{node}:{storage}",
                               async () => await (await GetPveClientAsync()).Nodes[node].Storage[storage].Content.GetAsync(),
                               30,
                               forceReload);

    public ValueTask ClearCacheAsync() => _fusionCache.RemoveByTagAsync(GetKeyPrefix());
}
