/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Module.Diagnostic.Services;

public interface IDiagnosticService
{
    Stream GenerateReport(JobResult result, ReportFormat format);
    string GetHelpUrl(JobDetail jobDetail);

    /// <summary>
    /// The page of this application that shows the resource a finding refers to, or null when
    /// there is none: findings about storage, users or cluster-wide settings have no page here,
    /// and the caller renders those as plain text.
    /// </summary>
    string? GetPveResourceUrl(string idResource,
                              Corsinvest.ProxmoxVE.Diagnostic.Api.DiagnosticResultContext context,
                              string clusterName);

    /// <summary>
    /// Builds a human-readable label from the raw <c>idResource</c> path (es. "nodes/cc01/qemu/100")
    /// and the diagnostic context. Falls back to the raw path when the parts don't match the expected shape.
    /// </summary>
    string GetResourceLabel(string idResource, Corsinvest.ProxmoxVE.Diagnostic.Api.DiagnosticResultContext context, string clusterName);
}
