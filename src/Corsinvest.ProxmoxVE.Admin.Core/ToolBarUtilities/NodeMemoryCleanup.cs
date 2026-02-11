using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;

namespace Corsinvest.ProxmoxVE.Admin.Core.ToolBarUtilities;

public class NodeMemoryCleanup(IAdminService adminService,
                               IStringLocalizer<NodeMemoryCleanup> L,
                               NotificationService notificationService) : IToolBarUtility<IClusterResourceNode>
{
    public string Icon { get; } = "cleaning_services";
    public string Text { get; } = "Memory Cleanup";
    public ToolBarUtilityType Type { get; } = ToolBarUtilityType.Node;
    public bool RequireConfirm { get; } = true;
    public bool IsVIsible(IClusterResourceNode item) => true;

    public async Task ExecuteAsync(string clusterName, IClusterResourceNode item)
    {
        await using var webTerm = new PveWebTermClient(await adminService[clusterName].GetPveClientAsync(), item.Node);
        await webTerm.ConnectAsync();

        var command = "sync && echo 3 > /proc/sys/vm/drop_caches && echo 1 > /proc/sys/vm/compact_memory";
        var (_, stdErr, exitCode) = await webTerm.ExecuteCommandAsync(command);
        if (exitCode == 0)
        {
            notificationService.Info(L["Node memory freed successfully!"]);
        }
        else
        {
            notificationService.Error(stdErr);
        }
    }
}
