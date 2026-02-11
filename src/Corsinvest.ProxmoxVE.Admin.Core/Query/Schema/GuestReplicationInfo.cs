using System.ComponentModel.DataAnnotations.Schema;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Node;

namespace Corsinvest.ProxmoxVE.Admin.Core.Query.Schema;

/// <summary>
/// Guest replication information
/// </summary>
[Table("guestReplications")]
[Description("Guest Replication")]
public class GuestReplicationInfo
{
    [Field(AllowedValues = ["qemu", "lxc"])]
    public string VirtualizationType { get; set; } = default!;

    public long GuestId { get; set; }

    public string Comment { get; set; } = default!;

    public bool Disable { get; set; }

    public int FailCount { get; set; }

    public int Rate { get; set; }

    public string NodeNameSource { get; set; } = default!;

    public string JobNum { get; set; } = default!;

    public string Id { get; set; } = default!;

    public DateTime LastSync { get; set; }

    public DateTime NextSync { get; set; }

    public string Schedule { get; set; } = default!;

    public TimeSpan Duration { get; set; }

    public DateTime LastTry { get; set; }

    public string NodeNameTarget { get; set; } = default!;

    public string StatusMessage { get; set; } = default!;

    public bool IsOk { get; set; }

    public static GuestReplicationInfo Map(NodeReplication replication)
        => new()
        {
            Comment = replication.Comment,
            Disable = replication.Disable,
            Duration = TimeSpan.FromSeconds(replication.Duration),
            IsOk = string.IsNullOrEmpty(replication.Error),
            StatusMessage = string.IsNullOrEmpty(replication.Error) ? "✅️ OK" : $"⚠️ {replication.Error}",
            FailCount = replication.FailCount,
            GuestId = long.TryParse(replication.Guest, out var guestId) ? guestId : 0,
            Id = replication.Id,
            JobNum = replication.JobNum,
            LastSync = DateTimeOffset.FromUnixTimeSeconds(replication.LastSync).DateTime,
            LastTry = DateTimeOffset.FromUnixTimeSeconds(replication.LastTry).DateTime,
            NextSync = DateTimeOffset.FromUnixTimeSeconds(replication.NextSync).DateTime,
            Rate = replication.Rate,
            VirtualizationType = replication.VmType,
            Schedule = replication.Schedule,
            NodeNameSource = replication.Source,
            NodeNameTarget = replication.Target
        };
}
