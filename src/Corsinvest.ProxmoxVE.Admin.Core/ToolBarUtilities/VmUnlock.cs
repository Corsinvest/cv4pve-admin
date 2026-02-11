using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;

namespace Corsinvest.ProxmoxVE.Admin.Core.ToolBarUtilities;

public class VmUnlock(IAdminService adminService) : IToolBarUtility<IClusterResourceVm>
{
    public string Icon { get; } = "lock_open";
    public string Text { get; } = "Unlock";
    public ToolBarUtilityType Type { get; } = ToolBarUtilityType.Vm;
    public bool RequireConfirm { get; } = true;
    public bool IsVIsible(IClusterResourceVm item) => item.IsLocked;

    public async Task ExecuteAsync(string clusterName, IClusterResourceVm item)
    {
        var client = await adminService[clusterName].GetPveClientAsync();
        await client.VmUnlockAsync(item.Node, item.VmType, item.VmId);
    }
}
