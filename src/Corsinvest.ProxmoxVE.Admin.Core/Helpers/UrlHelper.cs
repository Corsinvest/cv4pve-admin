/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;

namespace Corsinvest.ProxmoxVE.Admin.Core.Helpers;

public static class UrlHelper
{
    public const string ModuleComponentUrl = "/module/";
    public static string SystemTasksUrl { get; set; } = default!;
    public static string UrlNewPveConfig { get; set; } = default!;
    public static string UrlChangePassword { get; set; } = default!;

    public static string GetPveUrl(string baseAddress, string id) => $"{baseAddress}/#v1:0:={id}";

    public static string ModuleUrl(string slug, string clusterName) => $"{ModuleComponentUrl}{clusterName}/{slug}";

    public static string? GetClusterNameFromUrl(string url)
    {
        var absolutePath = new Uri(url).AbsolutePath;
        if (!absolutePath.StartsWith(ModuleComponentUrl)) { return null; }

        var segments = absolutePath[ModuleComponentUrl.Length..].Split('/');
        return segments.Length > 0 && !string.IsNullOrEmpty(segments[0]) ? segments[0] : null;
    }

    public static class Resources
    {
        public static string GetUrl(ClusterResource item, string clusterName)
             => item.ResourceType switch
             {
                 ClusterResourceType.Node => NodeUrl(item.Node, clusterName),
                 ClusterResourceType.Vm => VmUrl(item.VmId, clusterName),
                 ClusterResourceType.Unknown or ClusterResourceType.Storage or ClusterResourceType.Pool
                    or ClusterResourceType.Sdn or ClusterResourceType.All => "",

                 _ => "",
             };

        public static string VmUrl(long vmId, string clusterName)
            => $"{ModuleUrl("resources", clusterName)}/vms?vmid={vmId}";
        public static string NodeUrl(string node, string clusterName)
            => $"{ModuleUrl("resources", clusterName)}/nodes?node={node}";
    }
}
