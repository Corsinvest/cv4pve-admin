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

    public static async Task<bool> HasNodeAsync(this IPermissionService permissionService,
                                                string clusterName,
                                                Permission permission,
                                                string node)
        => await permissionService.HasAsync(clusterName, permission.Key, PermissionHelper.GetPathNode(node));

    public static async Task<bool> HasVmAsync(this IPermissionService permissionService,
                                              string clusterName,
                                              Permission permission,
                                              long vmId)
        => await permissionService.HasAsync(clusterName, permission.Key, PermissionHelper.GetPathVm(vmId));
}
