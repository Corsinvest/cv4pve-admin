using System.ComponentModel.DataAnnotations.Schema;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;

namespace Corsinvest.ProxmoxVE.Admin.Core.Query.Schema;

/// <summary>
/// Guest (VM/Container) information
/// </summary>
[Table("guests")]
[Description("Guest")]
public class GuestInfo
{
    [Field(DefaultFieldSelection = 1)]
    public long Id { get; set; }

    [Field(DefaultFieldSelection = 2)]
    public string Name { get; set; } = default!;

    public string PoolName { get; set; } = default!;

    [Field(DefaultFieldSelection = 3)]
    public string NodeName { get; set; } = default!;

    [Field(DefaultFieldSelection = 4, AllowedValues = ["unknown", "stopped", "running"])]
    public string Status { get; set; } = default!;

    public string Tags { get; set; } = default!;

    public bool IsTemplate { get; set; }

    public bool IsLocked { get; set; }

    [Field(AllowedValues = ["backup", "clone", "create", "migrate", "rollback", "snapshot", "snapshot-delete", "suspending", "suspended"])]
    public string LockedForAction { get; set; } = default!;

    [Field(DefaultFieldSelection = 5, DataFormatString = FormatHelper.FormatUnixTime)]
    public long Uptime { get; set; }

    [Field(AllowedValues = ["qemu", "lxc"])]
    public string VirtualizationType { get; set; } = default!;

    public long CpuCount { get; set; }

    [Field(DefaultFieldSelection = 6, DataFormatString = "P1")]
    public double CpuUsagePercentage { get; set; }

    [Field(DataFormatString = FormatHelper.FormatBytes)]
    public long AvailableMemoryBytes { get; set; }

    [Field(DataFormatString = FormatHelper.FormatBytes)]
    public long UsedMemoryBytes { get; set; }

    [Field(DefaultFieldSelection = 7, DataFormatString = "P1")]
    public double MemoryUsagePercentage { get; set; }

    [Field(DataFormatString = FormatHelper.FormatBytes)]
    public long AvailableDiskBytes { get; set; }

    [Field(DataFormatString = FormatHelper.FormatBytes)]
    public long UsedDiskBytes { get; set; }

    [Field(DataFormatString = "P1")]
    public double DiskUsagePercentage { get; set; }

    [Field(DataFormatString = FormatHelper.FormatBytes)]
    public long NetworkTrafficInByte { get; set; }

    [Field(DataFormatString = FormatHelper.FormatBytes)]
    public long NetworkTrafficOutByte { get; set; }

    [Field(DataFormatString = FormatHelper.FormatBytes)]
    public long DiskReadBytes { get; set; }

    [Field(DataFormatString = FormatHelper.FormatBytes)]
    public long DiskWriteBytes { get; set; }

    [Field(DataFormatString = "P1")]
    public double MemoryUsagePercentageByGuestOnNode { get; set; }

    public static GuestInfo Map(ClusterResource item)
        => new()
        {
            Id = item.VmId,
            Name = item.Name,
            NodeName = item.Node,
            PoolName = item.Pool,
            Status = item.Status,
            Tags = item.Tags,
            IsTemplate = item.IsTemplate,
            IsLocked = item.IsLocked,
            LockedForAction = item.Lock,
            Uptime = item.Uptime,
            VirtualizationType = item.Type,
            CpuCount = item.CpuSize,
            CpuUsagePercentage = item.CpuUsagePercentage * 100.0,
            AvailableMemoryBytes = Convert.ToInt64(item.MemorySize),
            UsedMemoryBytes = Convert.ToInt64(item.MemoryUsage),
            MemoryUsagePercentage = item.MemoryUsagePercentage * 100.0,
            AvailableDiskBytes = Convert.ToInt64(item.DiskSize),
            UsedDiskBytes = Convert.ToInt64(item.DiskUsage),
            DiskUsagePercentage = item.DiskUsagePercentage * 100.0,
            NetworkTrafficInByte = item.NetIn,
            NetworkTrafficOutByte = item.NetOut,
            DiskReadBytes = item.DiskRead,
            DiskWriteBytes = item.DiskWrite,
            MemoryUsagePercentageByGuestOnNode = item.HostMemoryUsage * 100.0
        };
}
