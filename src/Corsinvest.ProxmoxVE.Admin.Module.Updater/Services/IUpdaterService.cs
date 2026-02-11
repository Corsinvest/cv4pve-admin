using Corsinvest.ProxmoxVE.Admin.Module.Updater.Models;

namespace Corsinvest.ProxmoxVE.Admin.Module.Updater.Services;

public interface IUpdaterService
{
    MemoryStream GeneratePdf(string clusterName, IEnumerable<ClusterResourceUpdateScanInfo> items);
}
