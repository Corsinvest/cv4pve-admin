/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.ComponentModel.DataAnnotations.Schema;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;

namespace Corsinvest.ProxmoxVE.Admin.Core.Query.Schema;

/// <summary>
/// Cluster node information
/// </summary>
[Table("nodes")]
[Description("Node")]
public class NodeInfo
{
    [Field(DefaultFieldSelection = 1)]
    public string Name { get; set; } = default!;

    [Field(DefaultFieldSelection = 2, AllowedValues = ["unknown", "online", "offline"])]
    public string Status { get; set; } = default!;

    [Field(DefaultFieldSelection = 3, DataFormatString = FormatHelper.FormatUnixTime)]
    public long Uptime { get; set; }

    public long CpuCount { get; set; }

    [Field(DefaultFieldSelection = 4, DataFormatString = "P1")]
    public double CpuUsagePercentage { get; set; }

    [Field(DefaultFieldSelection = 5, DataFormatString = FormatHelper.FormatBytes)]
    public long AvailableMemoryBytes { get; set; }

    [Field(DefaultFieldSelection = 6, DataFormatString = FormatHelper.FormatBytes)]
    public long UsedMemoryBytes { get; set; }

    [Field(DefaultFieldSelection = 7, DataFormatString = "P1")]
    public double MemoryUsagePercentage { get; set; }

    public static NodeInfo Map(ClusterResource item)
        => new()
        {
            Name = item.Node,
            Status = item.Status,
            Uptime = item.Uptime,
            CpuCount = item.CpuSize,
            CpuUsagePercentage = item.CpuUsagePercentage * 100.0,
            AvailableMemoryBytes = Convert.ToInt64(item.MemorySize),
            UsedMemoryBytes = Convert.ToInt64(item.MemoryUsage),
            MemoryUsagePercentage = item.MemoryUsagePercentage * 100.0
        };
}
