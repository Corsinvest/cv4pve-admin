using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components.ProxmoxVE.Vm;

public partial class QemuGuestInfo(IAdminService adminService) : /*IRefreshableData,*/ IClusterName
{
    [EditorRequired, Parameter] public IClusterResourceVm Vm { get; set; } = default!;
    [EditorRequired, Parameter] public string ClusterName { get; set; } = default!;

    private VmQemuAgentOsInfo.ResultInt? OsInfo { get; set; }
    private VmQemuAgentGetFsInfo? FsInfo { get; set; }
    private VmQemuAgentNetworkGetInterfaces? NetworkGetInterfaces { get; set; }
    private bool QemuAgentRunning { get; set; }
    private string? HostName { get; set; }
    private bool IsLoading { get; set; }

    protected override async Task OnInitializedAsync() => await RefreshDataAsync();

    private async Task RefreshDataAsync()
    {
        IsLoading = true;
        if (Vm.VmType == VmType.Qemu && Vm.IsRunning)
        {
            var client = await adminService[ClusterName].GetPveClientAsync();
            var vm = client.Nodes[Vm.Node].Qemu[Vm.VmId];
            var ping = await vm.Agent.Ping.Ping();
            QemuAgentRunning = ping.IsSuccessStatusCode;

            if (QemuAgentRunning)
            {
                try
                {
                    FsInfo = await vm.Agent.GetFsinfo.GetAsync();
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, "Failed to get filesystem info from QEMU agent for VM {VmId} on node {Node}",
                        Vm.VmId, Vm.Node);
                }

                try
                {
                    HostName = (await vm.Agent.GetHostName.GetAsync())?.Result?.HostName;
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, "Failed to get hostname from QEMU agent for VM {VmId} on node {Node}",
                        Vm.VmId, Vm.Node);
                }

                try
                {
                    OsInfo = (await vm.Agent.GetOsinfo.GetAsync())?.Result;
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, "Failed to get OS info from QEMU agent for VM {VmId} on node {Node}",
                        Vm.VmId, Vm.Node);
                }

                try
                {
                    NetworkGetInterfaces = await vm.Agent.NetworkGetInterfaces.GetAsync();
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, "Failed to get network interfaces from QEMU agent for VM {VmId} on node {Node}",
                        Vm.VmId, Vm.Node);
                }
            }
        }

        IsLoading = false;

        await InvokeAsync(StateHasChanged);
    }
}
