/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.Domain.Entities;
using Corsinvest.AppHero.Core.Domain.Models;
using Corsinvest.ProxmoxVE.Admin.Core.Repository;

namespace Corsinvest.ProxmoxVE.Admin.AutoSnap.Models;

public class AutoSnapJob : JobSchedule, IAggregateRoot<int>, IClusterName
{
    [Required]
    public int Id { get; set; }

    [Required]
    public string ClusterName { get; set; } = default!;

    [Required]
    public string VmIds { get; set; } = default!;

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