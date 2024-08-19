/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.Domain.Entities;
using Corsinvest.ProxmoxVE.Api.Shared.Utils;
using System.ComponentModel.DataAnnotations.Schema;

namespace Corsinvest.ProxmoxVE.Admin.VzDumpTrend.Models;

public class VzDumpDetail : EntityBase<int>
{
    [Required]
    public VzDumpTask Task { get; set; } = default!;

    public DateTime? Start { get; set; }

    public DateTime? End { get; set; }
    public double Duration
        => End.HasValue && Start.HasValue
            ? (End - Start).Value.TotalSeconds
            : 0;

    [Display(Name = "Duration")]
    public string DurationText
        => End.HasValue && Start.HasValue
            ? (End - Start).Value.ToString("hh':'mm':'ss")
            : "00:00:00";

    public string? VmId { get; set; }
    public double Size { get; set; }

    [Display(Name = "Size")]
    public string SizeString => FormatHelper.FromBytes(Size);

    public string? Error { get; set; }
    public bool Status { get; set; }
    public double TransferSize { get; set; }
    public double TransferSpeed => Duration > 0 ? TransferSize / Duration : 0;

    [Display(Name = "Transfer Speed")]
    public string TransferSpeedText => FormatHelper.FromBytes(TransferSize);

    public string? Archive { get; set; }

    [NotMapped]
    public List<string> Logs { get; set; } = default!;

    [NotMapped]
    public string? Node => Task?.Node;

    [NotMapped]
    public string? Storage => Task?.Storage;

}
