/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.Domain.Entities;
using Corsinvest.AppHero.Core.Domain.Models;
using Corsinvest.AppHero.Core.Extensions;
using Corsinvest.ProxmoxVE.Admin.Core.Repository;
using System.ComponentModel.DataAnnotations.Schema;

namespace Corsinvest.ProxmoxVE.Admin.AutoSnap.Models;

public class AutoSnapJob : JobSchedule, IAggregateRoot<int>, IClusterName
{
    public AutoSnapJob() => CronExpression = "*/15 * * * *";

    [Required]
    public int Id { get; set; }

    [Required]
    public string ClusterName { get; set; } = default!;

    [Required]
    public string VmIds { get; set; } = default!;

    [NotMapped]
    public IEnumerable<string> VmIdsList
    {
        get => string.IsNullOrEmpty(VmIds)
                ? Enumerable.Empty<string>()
                : VmIds.Split(",").AsEnumerable();

        set => VmIds = value.Order().JoinAsString(",");
    }

    [Required]
    [RegularExpression("^[a-zA-Z0-9]+$")]
    public string Label { get; set; } = default!;

    [Required]
    [Range(1, 100)]
    public int Keep { get; set; }

    public bool VmStatus { get; set; }
    public bool OnlyRuns { get; set; }

    [Required]
    public long TimeoutSnapshot { get; set; } = 30;

    public List<AutoSnapJobHistory> Histories { get; set; } = new();
    public List<AutoSnapJobHook> Hooks { get; set; } = new();
    public DateTimeOffset? LastRunTime => Histories?.LastOrDefault()?.Start;
}