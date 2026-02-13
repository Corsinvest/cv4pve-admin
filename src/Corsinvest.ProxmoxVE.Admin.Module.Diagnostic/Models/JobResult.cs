/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Module.Diagnostic.Models;

public class JobResult : IClusterName, IId
{
    public int Id { get; set; }
    [Required] public string ClusterName { get; set; } = default!;
    public int Warning { get; set; }
    public int Critical { get; set; }
    public int Info { get; set; }
    public DateTime Start { get; set; }
    public DateTime? End { get; set; }

    public TimeSpan Duration
        => End.HasValue
            ? (End - Start).Value
            : TimeSpan.Zero;

    public List<JobDetail> Details { get; set; } = [];
}
