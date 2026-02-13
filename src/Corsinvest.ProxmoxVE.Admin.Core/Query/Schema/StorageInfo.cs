/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.ComponentModel.DataAnnotations.Schema;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;

namespace Corsinvest.ProxmoxVE.Admin.Core.Query.Schema;

/// <summary>
/// Storage information
/// </summary>
[Table("storages")]
[Description("Storage")]
public class StorageInfo
{
    [Field(DefaultFieldSelection = 1)]
    public string Name { get; set; } = default!;

    public string PoolName { get; set; } = default!;

    [Field(DefaultFieldSelection = 2)]
    public string NodeName { get; set; } = default!;

    public bool IsShared { get; set; }

    [Field(DefaultFieldSelection = 3, AllowedValues = ["images", "import", "rootdir", "iso", "vztmpl", "backup"])]
    public string Content { get; set; } = default!;

    [Field(DefaultFieldSelection = 4, AllowedValues = ["unknown", "available"])]
    public string Status { get; set; } = default!;

    [Field(DefaultFieldSelection = 5, AllowedValues = ["btrfs", "cephfs", "cifs", "dir", "esxi", "glusterfs", "iscsi", "iscsidirect", "lvm", "lvmthin", "nfs", "pbs", "rbd", "zfs", "zfspool"])]
    public string Type { get; set; } = default!;

    public long AvailableBytes { get; set; }

    public long UsedBytes { get; set; }

    [Field(DefaultFieldSelection = 6)]
    public double UsagePercentage { get; set; }

    public static StorageInfo Map(ClusterResource item)
        => new()
        {
            Name = item.Storage,
            NodeName = item.Node,
            PoolName = item.Pool,
            Status = item.Status,
            Type = item.PluginType,
            Content = item.Content,
            IsShared = item.Shared,
            AvailableBytes = Convert.ToInt64(item.DiskSize),
            UsedBytes = Convert.ToInt64(item.DiskUsage),
            UsagePercentage = item.DiskUsagePercentage * 100.0
        };
}
