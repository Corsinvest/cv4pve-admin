/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.ComponentModel.DataAnnotations.Schema;

namespace Corsinvest.ProxmoxVE.Admin.Module.AutoSnap.Models;

public class JobSchedule : JobScheduleBase, IClusterName, IId, IDescription
{
    public JobSchedule() => CronExpression = "*/15 * * * *";

    public int Id { get; set; }
    [Required] public string ClusterName { get; set; } = default!;
    [Required] public string Description { get; set; } = default!;
    [Required] public string VmIds { get; set; } = default!;

    [NotMapped]
    public IEnumerable<string> VmIdsList
    {
        get => string.IsNullOrEmpty(VmIds)
                ? []
                : VmIds.Split(",").AsEnumerable();

        set => VmIds = (value ?? []).Order().JoinAsString(",");
    }

    [Required, RegularExpression("^[a-zA-Z0-9]+$")]
    public string Label { get; set; } = default!;

    [Range(1, 100)]
    public int Keep { get; set; }

    [Display(Name = "Include RAM")]
    public bool VmStatus { get; set; }

    [Display(Name = "While VM/CT Running")]
    public bool OnlyRuns { get; set; }

    public long TimeoutSnapshot { get; set; } = 30;
    public ICollection<JobResult> Results { get; set; } = [];
    public ICollection<JobWebHook> WebHooks { get; set; } = [];
    public DateTimeOffset? LastRunTime => Results?.LastOrDefault()?.Start;
}
