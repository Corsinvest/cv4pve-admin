/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.Domain.Entities;

namespace Corsinvest.ProxmoxVE.Admin.VzDumpTrend.Models;

public class VzDumpTask : EntityBase<int>, IClusterName
{
    [Required]
    public string ClusterName { get; set; } = default!;

    public DateTime Start { get; set; }
    public DateTime? End { get; set; }
    public double Duration => End.HasValue ? (End - Start).Value.TotalSeconds : 0;
    public string? TaskId { get; set; }
    public string? Status { get; set; }
    public string? Node { get; set; }
    public string? Log { get; set; }
    public string? Storage { get; set; }
    public List<VzDumpDetail> Details { get; set; } = new List<VzDumpDetail>();
}


