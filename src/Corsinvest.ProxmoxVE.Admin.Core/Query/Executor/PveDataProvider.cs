/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Corsinvest.ProxmoxVE.Admin.Core.Query.Schema;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;

namespace Corsinvest.ProxmoxVE.Admin.Core.Query.Executor;

/// <summary>
/// Provides queryable data from Proxmox VE with caching
/// </summary>
/// <param name="clusterClient">The cluster to read from</param>
/// <param name="filter">Keeps the nodes, guests and storages the caller may see. Null: all of them</param>
public class PveDataProvider(ClusterClient clusterClient,
                             Func<IEnumerable<ClusterResource>, Task<IEnumerable<ClusterResource>>>? filter = null) : IDataProvider
{
    private IReadOnlyList<ClusterResource>? _resources;

    // Every table is built from these: what is not here is in no table
    private async Task<IReadOnlyList<ClusterResource>> GetResourcesAsync()
    {
        if (_resources == null)
        {
            var resources = await clusterClient.CachedData.GetResourcesAsync(false);
            _resources = [.. filter == null ? resources : await filter(resources)];
        }

        return _resources;
    }

    private async Task<HashSet<string>> GetVmIdsAsync()
        => [.. (await GetResourcesAsync()).Where(a => a.ResourceType == ClusterResourceType.Vm).Select(a => a.VmId.ToString())];

    public async Task<IQueryable> GetAsync(string tableName)
        => tableName switch
        {
            var t when t == GetTableName<NodeInfo>() => await GetNodesAsync(),
            var t when t == GetTableName<GuestInfo>() => await GetGuestsAsync(),
            var t when t == GetTableName<StorageInfo>() => await GetStoragesAsync(),
            var t when t == GetTableName<GuestReplicationInfo>() => await GetGuestReplicationInfo(),
            var t when t == GetTableName<GuestSnapshotInfo>() => await GetSnapshotsAsync(),
            var t when t == GetTableName<StorageContentInfo>() => await GetStorageContentsAsync(),
            var t when t == GetTableName<ClusterReplicationInfo>() => await GetReplicationsAsync(),
            var t when t == GetTableName<QemuConfigInfo>() => await GetQemuConfigsAsync(),
            var t when t == GetTableName<LxcConfigInfo>() => await GetLxcConfigsAsync(),
            _ => throw new NotSupportedException($"Table '{tableName}' not supported")
        };

    private static string GetTableName<T>() => typeof(T).GetCustomAttribute<TableAttribute>()!.Name;

    private async Task<IQueryable<NodeInfo>> GetNodesAsync()
        => (await GetResourcesAsync())
            .Where(r => r.ResourceType == ClusterResourceType.Node)
            .Select(NodeInfo.Map)
            .AsQueryable();

    private async Task<IQueryable<GuestInfo>> GetGuestsAsync()
        => (await GetResourcesAsync())
            .Where(r => r.ResourceType == ClusterResourceType.Vm)
            .Select(GuestInfo.Map)
            .AsQueryable();

    private async Task<IQueryable<StorageInfo>> GetStoragesAsync()
        => (await GetResourcesAsync())
            .Where(r => r.ResourceType == ClusterResourceType.Storage)
            .Select(StorageInfo.Map)
            .AsQueryable();

    private async Task<IQueryable<ClusterReplicationInfo>> GetReplicationsAsync()
    {
        var vmIds = await GetVmIdsAsync();
        return (await clusterClient.CachedData.GetClusterReplicationsAsync(false))
                .Where(a => vmIds.Contains(a.Guest + string.Empty))
                .Select(ClusterReplicationInfo.Map)
                .AsQueryable();
    }

    private async Task<IQueryable<GuestReplicationInfo>> GetGuestReplicationInfo()
    {
        var vmIds = await GetVmIdsAsync();
        return (await GetResourcesAsync())
                .Where(a => a.ResourceType == ClusterResourceType.Node)
                .Select(node => AsyncHelper.RunSync(() => clusterClient.CachedData.GetReplicationsAsync(node.Node, null, false).AsTask()))
                .SelectMany(replications => replications.Where(a => vmIds.Contains(a.Guest + string.Empty)).Select(GuestReplicationInfo.Map))
                .AsQueryable();
    }

    private async Task<IQueryable<QemuConfigInfo>> GetQemuConfigsAsync()
        => (await GetResourcesAsync())
            .Where(r => r.ResourceType == ClusterResourceType.Vm && r.VmType == VmType.Qemu)
            .Select(vm => new QemuConfigInfo(vm.VmId, vm.Node, clusterClient.CachedData))
            .AsQueryable();

    private async Task<IQueryable<LxcConfigInfo>> GetLxcConfigsAsync()
        => (await GetResourcesAsync())
            .Where(r => r.ResourceType == ClusterResourceType.Vm && r.VmType == VmType.Lxc)
            .Select(vm => new LxcConfigInfo(vm.VmId, vm.Node, clusterClient.CachedData))
            .AsQueryable();

    private async Task<IQueryable<GuestSnapshotInfo>> GetSnapshotsAsync()
    {
        var disks = await clusterClient.CachedData.GetStorageSnapshotInfosAsync(false);

        return (await GetResourcesAsync())
                .Where(r => r.ResourceType == ClusterResourceType.Vm)
                .SelectMany(vm => AsyncHelper.RunSync(() =>
                    clusterClient.CachedData.GetSnapshotsAsync(vm.Node, vm.VmType, vm.VmId, false).AsTask())
                    .Select(s => GuestSnapshotInfo.Map(s, vm.VmId, vm.Node, vm.Type, StorageSnapshotHelper.CalculateSnapshots(vm.VmId, s.Name, disks))))
                .AsQueryable();
    }

    private async Task<IQueryable<StorageContentInfo>> GetStorageContentsAsync()
    {
        var resources = await GetResourcesAsync();
        var nodes = resources.Where(r => r.ResourceType == ClusterResourceType.Node);
        var storages = resources.Where(r => r.ResourceType == ClusterResourceType.Storage);

        return nodes.SelectMany(node =>
                storages.Where(s => s.Node == node.Node)
                        .SelectMany(storage => AsyncHelper.RunSync(() => clusterClient.CachedData.GetStorageContentsAsync(node.Node, storage.Storage, false).AsTask())
                        .Select(content => StorageContentInfo.Map(content, node.Node, storage.Storage))))
                    .AsQueryable();
    }
}
