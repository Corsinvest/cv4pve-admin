/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Corsinvest.ProxmoxVE.Admin.Core.Configuration;

namespace Corsinvest.ProxmoxVE.Admin.Module.AutoSnap.Models;

public class JobSchedule : JobScheduleBase, IClusterName, IId, IDescription
{
    public JobSchedule() => CronExpression = "*/15 * * * *";

    public int Id { get; set; }
    [Required] public string ClusterName { get; set; } = default!;
    [Required] public string Description { get; set; } = default!;
    [Required] public string VmIds { get; set; } = default!;

    [Required, RegularExpression("^[a-zA-Z0-9]+$")]
    public string Label { get; set; } = default!;

    [Range(1, 100)]
    public int Keep { get; set; }

    public bool VmStatus { get; set; }
    public bool OnlyRuns { get; set; }

    public long TimeoutSnapshot { get; set; } = 30;
    public ICollection<JobResult> Results { get; set; } = [];
    public DateTimeOffset? LastRunTime => Results?.LastOrDefault()?.Start;

    [NotMapped]
    public ExtendedData ExtendedData { get; set; } = [];

    public string ExtendedDataJson
    {
        get => JsonSerializer.Serialize(ExtendedData);
        set => ExtendedData = JsonSerializer.Deserialize<ExtendedData>(value) ?? [];
    }
}
