/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth;
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth.Permissions;
using Corsinvest.ProxmoxVE.Diagnostic.Api;
using CoreHelpers = Corsinvest.ProxmoxVE.Admin.Core.Helpers;

namespace Corsinvest.ProxmoxVE.Admin.Module.Diagnostic.Helpers;

public static class DiagnosticPermissions
{
    // A finding is readable with Read on its node, guest or storage; a finding about the cluster itself,
    // or one whose resource cannot be read from its id, needs Read on the whole cluster
    public static async Task<List<T>> FilterAsync<T>(IPermissionService permissionService,
                                                     string clusterName,
                                                     IEnumerable<T> items,
                                                     Func<T, string?> idResource,
                                                     Func<T, DiagnosticResultContext> context)
    {
        if (await permissionService.HasClusterReadAsync(clusterName)) { return [.. items]; }

        var readable = new Dictionary<string, bool>();
        var result = new List<T>();
        foreach (var item in items)
        {
            if (GetPermission(idResource(item), context(item)) is not { } permission) { continue; }

            var key = permission.Key + permission.Path;
            if (!readable.TryGetValue(key, out var allowed))
            {
                allowed = await permissionService.HasAsync(clusterName, permission.Key, permission.Path);
                readable[key] = allowed;
            }
            if (allowed) { result.Add(item); }
        }
        return result;
    }

    private static (string Key, string Path)? GetPermission(string? idResource, DiagnosticResultContext context)
    {
        var parts = (idResource ?? string.Empty).Split('/');
        return context switch
        {
            DiagnosticResultContext.Node when parts.Length > 1
                => (ClusterPermissions.Node.Data.Read.Key, CoreHelpers.PermissionHelper.GetPathNode(parts[1])),

            DiagnosticResultContext.Qemu or DiagnosticResultContext.Lxc when parts.Length > 3 && long.TryParse(parts[3], out var vmId)
                => (ClusterPermissions.Vm.Data.Read.Key, CoreHelpers.PermissionHelper.GetPathVm(vmId)),

            DiagnosticResultContext.Storage when parts.Length > 1
                => (ClusterPermissions.Storage.Data.Read.Key, CoreHelpers.PermissionHelper.GetPathStorage(parts[^1])),

            _ => null
        };
    }
}
