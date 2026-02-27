/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;

namespace Corsinvest.ProxmoxVE.Admin.Core.Extensions;

public static class ClusterResourceExtensions
{
    public static string[] GetTagsArray(this ClusterResource resource)
        => SplitTags(resource.Tags);

    public static string[] SplitTags(string? tags)
        => string.IsNullOrWhiteSpace(tags)
            ? []
            : tags.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    public static IEnumerable<ResourceUsage> GetResourceUsage(this IEnumerable<ClusterResource> resources, IStringLocalizer L)
    {
        var nodes = resources.Where(a => a.ResourceType == ClusterResourceType.Node && a.IsOnline).ToList();

        //all storage not shared
        var allStorage = resources.Where(a => a.ResourceType == ClusterResourceType.Storage && a.IsAvailable);

        var storages = allStorage.Where(a => !a.Shared).ToList();
        storages.AddRange(allStorage.Where(a => a.Shared).DistinctBy(a => a.Storage));

        var memoryUsage = nodes.Sum(a => a.MemoryUsage);
        var memorySize = nodes.Sum(a => a.MemorySize);
        var diskUsage = storages.Sum(a => a.DiskUsage);
        var diskSize = storages.Sum(a => a.DiskSize);

        return
        [
            new()
            {
                Name = L["CPU"],
                Group="CPU",
                Usage = Math.Round(nodes.Average(a => a.CpuUsagePercentage) * 100, 1),
                Info = L["of {0} CPU(s)", nodes.Sum(a => a.CpuSize)]
            },
            new()
            {
                Name = L["Memory"],
                Group="Memory",
                Usage = Math.Round(Convert.ToDouble(memoryUsage) / memorySize * 100, 1),
                Info = L["{0} of {1}", FormatHelper.FromBytes(memoryUsage), FormatHelper.FromBytes(memorySize)]
            },
            new()
            {
                Name = L["Storage"],
                Group="Storage",
                Usage = Math.Round(Convert.ToDouble(diskUsage) / diskSize * 100, 1),
                Info = L["{0} of {1}", FormatHelper.FromBytes(diskUsage), FormatHelper.FromBytes(diskSize)]
            }
        ];
    }

}
