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
public class PveDataProvider(ClusterClient clusterClient) : IDataProvider
{
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
        => (await clusterClient.CachedData.GetResourcesAsync(false))
            .Where(r => r.ResourceType == ClusterResourceType.Node)
            .Select(NodeInfo.Map)
            .AsQueryable();

    private async Task<IQueryable<GuestInfo>> GetGuestsAsync()
        => (await clusterClient.CachedData.GetResourcesAsync(false))
            .Where(r => r.ResourceType == ClusterResourceType.Vm)
            .Select(GuestInfo.Map)
            .AsQueryable();

    private async Task<IQueryable<StorageInfo>> GetStoragesAsync()
        => (await clusterClient.CachedData.GetResourcesAsync(false))
            .Where(r => r.ResourceType == ClusterResourceType.Storage)
            .Select(StorageInfo.Map)
            .AsQueryable();

    private async Task<IQueryable<ClusterReplicationInfo>> GetReplicationsAsync()
        => (await clusterClient.CachedData.GetClusterReplicationsAsync(false))
            .Select(ClusterReplicationInfo.Map)
            .AsQueryable();

    private async Task<IQueryable<GuestReplicationInfo>> GetGuestReplicationInfo()
        => (await clusterClient.CachedData.GetResourcesAsync(false))
            .Where(a => a.ResourceType == ClusterResourceType.Node)
            .Select(node => AsyncHelper.RunSync(() => clusterClient.CachedData.GetReplicationsAsync(node.Node, null, false)))
            .SelectMany(replications => replications.Select(GuestReplicationInfo.Map))
            .AsQueryable();

    private async Task<IQueryable<QemuConfigInfo>> GetQemuConfigsAsync()
        => (await clusterClient.CachedData.GetResourcesAsync(false))
            .Where(r => r.ResourceType == ClusterResourceType.Vm && r.VmType == VmType.Qemu)
            .Select(vm => new QemuConfigInfo(vm.VmId, vm.Node, clusterClient.CachedData))
            .AsQueryable();

    private async Task<IQueryable<LxcConfigInfo>> GetLxcConfigsAsync()
        => (await clusterClient.CachedData.GetResourcesAsync(false))
            .Where(r => r.ResourceType == ClusterResourceType.Vm && r.VmType == VmType.Lxc)
            .Select(vm => new LxcConfigInfo(vm.VmId, vm.Node, clusterClient.CachedData))
            .AsQueryable();

    private async Task<IQueryable<GuestSnapshotInfo>> GetSnapshotsAsync()
        => (await clusterClient.CachedData.GetResourcesAsync(false))
            .Where(r => r.ResourceType == ClusterResourceType.Vm)
            .SelectMany(vm => AsyncHelper.RunSync(() =>
                clusterClient.CachedData.GetSnapshotsAsync(vm.Node, vm.VmType, vm.VmId, false))
                .Select(s => GuestSnapshotInfo.Map(s, vm.VmId, vm.Node, vm.Type)))
            .AsQueryable();

    private async Task<IQueryable<StorageContentInfo>> GetStorageContentsAsync()
    {
        var resources = await clusterClient.CachedData.GetResourcesAsync(false);
        var nodes = resources.Where(r => r.ResourceType == ClusterResourceType.Node);
        var storages = resources.Where(r => r.ResourceType == ClusterResourceType.Storage);

        return nodes.SelectMany(node =>
                storages.Where(s => s.Node == node.Node)
                        .SelectMany(storage => AsyncHelper.RunSync(() => clusterClient.CachedData.GetStorageContentsAsync(node.Node, storage.Storage, false))
                        .Select(content => StorageContentInfo.Map(content, node.Node, storage.Storage))))
                    .AsQueryable();
    }
}
