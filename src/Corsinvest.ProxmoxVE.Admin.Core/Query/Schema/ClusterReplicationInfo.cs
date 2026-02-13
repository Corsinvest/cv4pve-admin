/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.ComponentModel.DataAnnotations.Schema;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;

namespace Corsinvest.ProxmoxVE.Admin.Core.Query.Schema;

/// <summary>
/// Cluster replication configuration
/// </summary>
[Table("replications")]
[Description("Cluster Replication")]
public class ClusterReplicationInfo
{
    [Field(Description = "Storage replication schedule. The format is a subset of `systemd` calendar events.")]
    public string Schedule { get; set; } = default!;

    public string Id { get; set; } = default!;

    [Field(AllowedValues = ["local"])]
    public string Type { get; set; } = default!;

    public string Source { get; set; } = default!;

    public string Guest { get; set; } = default!;

    public string JobNum { get; set; } = default!;

    public string TargetNode { get; set; } = default!;

    public bool Disable { get; set; }

    public int RateLimit { get; set; }

    public static ClusterReplicationInfo Map(ClusterReplication replication)
        => new()
        {
            Schedule = replication.Schedule,
            Id = replication.Id,
            Type = replication.Type,
            Source = replication.Source,
            Guest = replication.Guest,
            JobNum = replication.JobNum,
            TargetNode = replication.Target,
            Disable = replication.Disable,
            RateLimit = 0 //replication.RateLimit
        };
}
