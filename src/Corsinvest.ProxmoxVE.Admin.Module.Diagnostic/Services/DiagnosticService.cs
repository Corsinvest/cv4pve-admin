namespace Corsinvest.ProxmoxVE.Admin.Module.Diagnostic.Services;

internal class DiagnosticService(IReportService reportService, IStringLocalizer<DiagnosticService> L) : IDiagnosticService
{
    public MemoryStream GeneratePdf(JobResult result)
        => reportService.GeneratePdf(L["Diagnostic result of cluster '{0}' Date {1}", result.ClusterName, result.Start]);

    public string GetHelpUrl(JobDetail jobDetail) => string.Empty;
}
