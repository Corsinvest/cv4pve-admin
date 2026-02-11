using Corsinvest.ProxmoxVE.Admin.Module.BackupAnalytics.Helpers;

namespace Corsinvest.ProxmoxVE.Admin.Module.BackupAnalytics;

internal class Job(IServiceScopeFactory scopeFactory)
{
    public async Task ScanAsync(string clusterName)
    {
        using var scope = scopeFactory.CreateScope();
        await ActionHelper.ScanAsync(scope, clusterName, true);
    }

    public async Task ScanFromResultAsync(string clusterName)
    {
        using var scope = scopeFactory.CreateScope();
        await ActionHelper.ScanAsync(scope, clusterName, false);
    }
}
