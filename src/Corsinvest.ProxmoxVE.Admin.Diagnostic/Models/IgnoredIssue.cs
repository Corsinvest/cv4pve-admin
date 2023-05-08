/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.Domain.Entities;
using Corsinvest.ProxmoxVE.Diagnostic.Api;

namespace Corsinvest.ProxmoxVE.Admin.Diagnostic.Models;

public class IgnoredIssue : EntityBase<int>, IClusterName
{
    [Required]
    public string ClusterName { get; set; } = default!;

    public string? IdResource { get; set; }
    public DiagnosticResultGravity Gravity { get; set; }
    public DiagnosticResultContext Context { get; set; }
    public string? SubContext { get; set; }
    public string? Description { get; set; }

    public DiagnosticResult ToDiagnosticResult()
        => new()
        {
            Context = Context,
            Description = Description,
            Gravity = Gravity,
            SubContext = SubContext,
            Id = IdResource
        };
}