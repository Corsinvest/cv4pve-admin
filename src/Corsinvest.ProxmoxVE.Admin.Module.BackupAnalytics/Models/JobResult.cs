/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Api.Shared.Utils;

namespace Corsinvest.ProxmoxVE.Admin.Module.BackupAnalytics.Models;

public class JobResult : IId
{
    public int Id { get; set; }
    [Required] public TaskResult TaskResult { get; set; } = default!;
    public DateTime? Start { get; set; }
    public DateTime? End { get; set; }

    public TimeSpan Duration
        => End.HasValue && Start.HasValue
            ? (End - Start).Value
            : TimeSpan.Zero;

    public string VmId { get; set; } = default!;

    [DisplayFormat(DataFormatString = FormatHelper.DataFormatBytes)]
    public double Size { get; set; }

    public string? Error { get; set; }
    public bool Status { get; set; }
    public double TransferSize { get; set; }

    public double TransferSpeed
        => Duration.TotalSeconds > 0
            ? TransferSize / Duration.TotalSeconds
           : 0;

    public string? Archive { get; set; }
    public string? Logs { get; set; }
}
