/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;
using Corsinvest.ProxmoxVE.Api.Shared.Utils;
using Microsoft.Extensions.Localization;

namespace Corsinvest.ProxmoxVE.Admin.Core.Models;

public class ResourceUsage
{
    public string Name { get; set; } = default!;
    public double Usage { get; set; }
    public string Info { get; set; } = default!;
    public string Color => PveBlazorHelper.GetColorRangeToString(Usage);

    public static IEnumerable<ResourceUsage> GetUsages(IEnumerable<ClusterResource> data, IStringLocalizer L)
    {
        var nodes = data.Where(a => a.ResourceType == ClusterResourceType.Node && a.IsOnline);

        //all storage not shaed
        var allStorage = data.Where(a => a.ResourceType == ClusterResourceType.Storage && a.IsAvailable);

        var storages = allStorage.Where(a => !a.Shared).ToList();
        storages.AddRange(allStorage.Where(a => a.Shared).DistinctBy(a => a.Storage));

        return new List<ResourceUsage>
        {
            new()
            {
                Name = L["CPU"],
                Usage = Math.Round(nodes.Average(a => a.CpuUsagePercentage) * 100, 1),
                Info = L["of {0} CPU(s)", nodes.Sum(a => a.CpuSize)]
            },

            new()
            {
                Name = L["Memory"],
                Usage = Math.Round(Convert.ToDouble(nodes.Sum(a => a.MemoryUsage)) / nodes.Sum(a => a.MemorySize) * 100, 1),
                Info = L["{0} of {1}",
                         FormatHelper.FromBytes(nodes.Sum(a => a.MemoryUsage)),
                         FormatHelper.FromBytes(nodes.Sum(a => a.MemorySize))]
            },

            new()
            {
                Name = L["Storage"],
                Usage = Math.Round(Convert.ToDouble(storages.Sum(a => a.DiskUsage)) / storages.Sum(a => a.DiskSize) * 100, 1),
                Info = L["{0} of {1}",
                         FormatHelper.FromBytes(storages.Sum(a => a.DiskUsage)),
                         FormatHelper.FromBytes(storages.Sum(a => a.DiskSize))]
            }
        };
    }
}
