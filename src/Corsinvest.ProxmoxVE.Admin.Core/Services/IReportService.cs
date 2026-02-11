namespace Corsinvest.ProxmoxVE.Admin.Core.Services;

public interface IReportService
{
    MemoryStream GeneratePdf(string title);
}
