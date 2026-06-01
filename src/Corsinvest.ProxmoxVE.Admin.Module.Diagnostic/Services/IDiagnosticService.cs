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
    /// Resolves the PVE-side URL for the resource referenced by a diagnostic finding.
    /// Returns "#" when the context does not have a direct PVE URL (Cluster / Storage / unknown).
    /// </summary>
    string GetPveResourceUrl(string idResource, Corsinvest.ProxmoxVE.Diagnostic.Api.DiagnosticResultContext context, string clusterName);
}
