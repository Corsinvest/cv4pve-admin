/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth;
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth.Permissions;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;

namespace Corsinvest.ProxmoxVE.Admin.Core.Extensions;

public static class PermissionExtensions
{
    //public static IEnumerable<Permission> CombineWith(this IEnumerable<Permission> first, IEnumerable<Permission> second)
    //    => first.Concat(second).Distinct();

    public static IEnumerable<Permission> CombineWith(this IEnumerable<Permission> first, PermissionsRead second)
        => first.Concat(second.Permissions).Distinct();

    public static IEnumerable<Permission> CombineWith(this IEnumerable<Permission> first, Role role)
        => first.Concat(role.Permissions).Distinct();

    public static IEnumerable<Permission> CombineWith(this IEnumerable<Permission> first, PermissionsCrud second)
        => first.Concat(second.Permissions).Distinct();

    public static IEnumerable<Permission> CombineWith(this IEnumerable<Permission> first, Permission single)
        => first.Append(single).Distinct();

    public static async Task<IEnumerable<ClusterResource>> FilterAsync(this IPermissionService permissionService,
                                                                       string clusterName,
                                                                       IEnumerable<ClusterResource> items)
    {
        var result = new List<ClusterResource>();
        foreach (var item in items)
        {
            if (await permissionService.HasAsync(clusterName, item))
            {
                result.Add(item);
            }
        }
        return result;
    }

    public static async Task<bool> HasAsync(this IPermissionService permissionService,
                                            string clusterName,
                                            ClusterResource item)
    {
        (var path, var permissionKey) = item.ResourceType switch
        {
            ClusterResourceType.All or ClusterResourceType.Sdn or ClusterResourceType.Pool or
            ClusterResourceType.Unknown => (string.Empty, string.Empty),

            ClusterResourceType.Node => (PermissionHelper.GetPathNode(item.Node), ClusterPermissions.Node.Data.Read.Key),
            ClusterResourceType.Vm => (PermissionHelper.GetPathVm(item.VmId), ClusterPermissions.Vm.Data.Read.Key),
            ClusterResourceType.Storage => (PermissionHelper.GetPathStorage(item.Storage), ClusterPermissions.Storage.Data.Read.Key),

            _ => (string.Empty, string.Empty)
        };

        return string.IsNullOrEmpty(path)
                || await permissionService.HasAsync(clusterName, permissionKey, path);
    }

    // A VM, a node and a storage that cannot exist: a permission that holds on them holds on any of their kind,
    // with the same rules (path, propagation, roles) as a check on a real one
    private const string AnyVmPath = "/vms/0";
    private const string AnyNodePath = "/nodes/~";
    private const string AnyStoragePath = "/storages/~";

    // Read on every VM, node and storage of the cluster: what is about the whole cluster (a job on all the VMs,
    // a scan, a total) is only for those who can see all of it
    public static async Task<bool> HasClusterReadAsync(this IPermissionService permissionService, string clusterName)
        => await permissionService.HasAsync(clusterName, ClusterPermissions.Vm.Data.Read.Key, AnyVmPath)
           && await permissionService.HasAsync(clusterName, ClusterPermissions.Node.Data.Read.Key, AnyNodePath)
           && await permissionService.HasAsync(clusterName, ClusterPermissions.Storage.Data.Read.Key, AnyStoragePath);

    public static async Task<List<T>> FilterByVmAsync<T>(this IPermissionService permissionService,
                                                         string clusterName,
                                                         IEnumerable<T> items,
                                                         Func<T, long> vmId)
    {
        if (await permissionService.HasAsync(clusterName, ClusterPermissions.Vm.Data.Read.Key, AnyVmPath)) { return [.. items]; }

        var readable = new Dictionary<long, bool>();
        var result = new List<T>();
        foreach (var item in items)
        {
            var id = vmId(item);
            if (!readable.TryGetValue(id, out var allowed))
            {
                allowed = await permissionService.HasAsync(clusterName, ClusterPermissions.Vm.Data.Read.Key, PermissionHelper.GetPathVm(id));
                readable[id] = allowed;
            }
            if (allowed) { result.Add(item); }
        }
        return result;
    }

    // A selection of VMs written as a list of ids, or with @all, pools, tags, nodes and ranges, which only the
    // cluster resolves: the explicit ids, or null when the selection is wider than a list (an empty one too)
    public static IReadOnlyList<long>? GetExplicitVmIds(string vmIds)
    {
        var ids = new List<long>();
        foreach (var token in vmIds.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (!token.All(char.IsAsciiDigit) || !long.TryParse(token, out var id)) { return null; }
            ids.Add(id);
        }
        return ids.Count == 0 ? null : ids;
    }

    // A selection the user may see: its VMs readable one by one, or Read on the whole cluster for a wider one
    public static async Task<bool> CanReadVmSelectionAsync(this IPermissionService permissionService, string clusterName, string vmIds)
        => GetExplicitVmIds(vmIds) is { } ids
            ? (await permissionService.FilterByVmAsync(clusterName, ids.Distinct(), a => a)).Count == ids.Distinct().Count()
            : await permissionService.HasClusterReadAsync(clusterName);

    // A selection the user may act on with a VM permission: on each of its VMs, or on all of them for a wider one
    public static async Task<bool> CanActOnVmSelectionAsync(this IPermissionService permissionService,
                                                            string clusterName,
                                                            Permission permission,
                                                            string vmIds)
    {
        if (GetExplicitVmIds(vmIds) is not { } ids)
        {
            return await permissionService.HasClusterReadAsync(clusterName)
                   && await permissionService.HasAsync(clusterName, permission.Key, AnyVmPath)
                   && (!ClusterPermissionMap.All.TryGetValue(permission.Key, out var pvePermission)
                       || await permissionService.PveHasAsync(clusterName, pvePermission, "/vms")
                       || await permissionService.PveHasAsync(clusterName, pvePermission, "/"));
        }

        foreach (var id in ids.Distinct())
        {
            if (!await permissionService.HasVmAsync(clusterName, permission, id)) { return false; }
        }
        return true;
    }

    public static async Task<bool> HasNodeAsync(this IPermissionService permissionService,
                                                string clusterName,
                                                Permission permission,
                                                string node)
    {
        if (!await permissionService.HasAsync(clusterName, permission.Key, PermissionHelper.GetPathNode(node))) { return false; }
        return !ClusterPermissionMap.All.TryGetValue(permission.Key, out var pvePermission)
               || await permissionService.PveHasNodeAsync(clusterName, pvePermission, node);
    }

    // The path of a storage on Proxmox VE is /storage/{id}, not the one the application uses for its own permissions
    public static async Task<bool> HasStorageAsync(this IPermissionService permissionService,
                                                   string clusterName,
                                                   Permission permission,
                                                   string storage)
    {
        if (!await permissionService.HasAsync(clusterName, permission.Key, PermissionHelper.GetPathStorage(storage))) { return false; }

        return !ClusterPermissionMap.All.TryGetValue(permission.Key, out var pvePermission)
               || await permissionService.PveHasAsync(clusterName, pvePermission, $"/storage/{storage}")
               || await permissionService.PveHasAsync(clusterName, pvePermission, "/storage")
               || await permissionService.PveHasAsync(clusterName, pvePermission, "/");
    }

    public static async Task<bool> HasVmAsync(this IPermissionService permissionService,
                                              string clusterName,
                                              Permission permission,
                                              long vmId)
    {
        if (!await permissionService.HasAsync(clusterName, permission.Key, PermissionHelper.GetPathVm(vmId))) { return false; }
        return !ClusterPermissionMap.All.TryGetValue(permission.Key, out var pvePermission)
               || await permissionService.PveHasVmAsync(clusterName, pvePermission, vmId);
    }

    public static async Task<bool> PveHasVmAsync(this IPermissionService permissionService,
                                                 string clusterName,
                                                 string pvePermission,
                                                 long vmId)
        => await permissionService.PveHasAsync(clusterName, pvePermission, PermissionHelper.GetPathVm(vmId))
           || await permissionService.PveHasAsync(clusterName, pvePermission, "/vms")
           || await permissionService.PveHasAsync(clusterName, pvePermission, "/");

    public static async Task<bool> PveHasNodeAsync(this IPermissionService permissionService,
                                                   string clusterName,
                                                   string pvePermission,
                                                   string node)
        => await permissionService.PveHasAsync(clusterName, pvePermission, PermissionHelper.GetPathNode(node))
           || await permissionService.PveHasAsync(clusterName, pvePermission, "/nodes")
           || await permissionService.PveHasAsync(clusterName, pvePermission, "/");
}
