namespace Corsinvest.ProxmoxVE.Admin.Module.Diagnostic.Services;

public interface IDiagnosticService
{
    MemoryStream GeneratePdf(JobResult result);
    string GetHelpUrl(JobDetail jobDetail);
}
