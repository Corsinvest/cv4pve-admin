using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;

namespace Corsinvest.ProxmoxVE.Admin.Module.Resources.Components.Widgets.Status;

public class Settings
{
    public VmType VmType { get; set; } = VmType.Qemu;
    public ClusterResourceType ResourceType { get; set; } = ClusterResourceType.Vm;
}
