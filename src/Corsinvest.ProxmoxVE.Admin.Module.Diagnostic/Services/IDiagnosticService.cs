/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Module.Diagnostic.Services;

public interface IDiagnosticService
{
    MemoryStream GeneratePdf(JobResult result);
    string GetHelpUrl(JobDetail jobDetail);
}
