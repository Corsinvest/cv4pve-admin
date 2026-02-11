using Corsinvest.ProxmoxVE.Admin.Module.NodeProtect.Folder.Helpers;

namespace Corsinvest.ProxmoxVE.Admin.Module.NodeProtect.Folder;

internal class Job(IServiceScopeFactory scopeFactory)
{
    public async Task BackupAsync(string clusterName)
    {
        using var scope = scopeFactory.CreateScope();
        await FolderHelper.BackupAsync(scope, clusterName);
    }
}
