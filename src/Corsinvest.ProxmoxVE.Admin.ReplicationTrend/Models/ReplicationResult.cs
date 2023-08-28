/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.Domain.Entities;
using Corsinvest.ProxmoxVE.Api.Shared.Utils;

namespace Corsinvest.ProxmoxVE.Admin.ReplicationTrend.Models;

public class ReplicationResult : EntityBase<int>, IClusterName
{
    [Required]
    public string ClusterName { get; set; } = default!;

    public string JobId { get; set; } = default!;
    public DateTime Start { get; set; }
    public DateTime? End { get; set; }
    public double DurationCalc => End.HasValue ? (End - Start).Value.TotalSeconds : 0;
    public double Duration { get; set; }

    [Display(Name = "Duration")]
    public string DurationText
        => End.HasValue
            ? (End - Start).Value.ToString("hh':'mm':'ss")
            : "00:00:00";

    public string VmId { get; set; } = default!;
    public double Size { get; set; }

    [Display(Name = "Size")]
    public string SizeString => FormatHelper.FromBytes(Size);

    public string Log { get; set; } = default!;
    public DateTime LastSync { get; set; }
    public string? Error { get; set; }
    public bool Status { get; set; }
}