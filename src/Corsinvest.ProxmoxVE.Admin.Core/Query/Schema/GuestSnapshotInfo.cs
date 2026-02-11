using System.ComponentModel.DataAnnotations.Schema;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;

namespace Corsinvest.ProxmoxVE.Admin.Core.Query.Schema;

/// <summary>
/// Guest snapshot information
/// </summary>
[Table("snapshots")]
[Description("Guest Snapshot")]
public class GuestSnapshotInfo
{
    public long GuestId { get; set; }

    public string NodeName { get; set; } = default!;

    [Field(AllowedValues = ["qemu", "lxc"])]
    public string VirtualizationType { get; set; } = default!;

    public string Parent { get; set; } = default!;

    public string Name { get; set; } = default!;

    public string Description { get; set; } = default!;

    public bool IncludeMemory { get; set; }

    public DateTime Timestamp { get; set; }

    public static GuestSnapshotInfo Map(VmSnapshot snapshot, long guestId, string nodeName, string virtualizationType)
        => new()
        {
            GuestId = guestId,
            NodeName = nodeName,
            VirtualizationType = virtualizationType,
            Description = snapshot.Description,
            IncludeMemory = snapshot.VmStatus,
            Name = snapshot.Name,
            Parent = snapshot.Parent,
            Timestamp = snapshot.Date
        };
}
