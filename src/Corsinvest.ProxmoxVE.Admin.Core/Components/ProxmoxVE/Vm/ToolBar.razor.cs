using Corsinvest.ProxmoxVE.Admin.Core.Commands;
using Corsinvest.ProxmoxVE.Admin.Core.Commands.Vm;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Common;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components.ProxmoxVE.Vm;

public partial class ToolBar(IBrowserService browserService,
                             IAdminService adminService,
                             DialogService dialogService,
                             ContextMenuService contextMenuService,
                             CommandExecutor commandExecutor,
                             IEnumerable<IToolBarUtility<IClusterResourceVm>> Utility) : IRefreshableData, IClusterName
{
    [EditorRequired, Parameter] public IClusterResourceVm Vm { get; set; } = default!;
    [EditorRequired, Parameter] public string ClusterName { get; set; } = default!;
    [Parameter] public bool CanConsole { get; set; }
    [Parameter] public bool CanChangeStatus { get; set; }
    [Parameter] public bool OnlyIcon { get; set; }
    [Parameter] public EventCallback<VmStatus> ChangeStatus { get; set; }

    private WebConsoleType DefaultWebConsoleType { get; set; } = WebConsoleType.NoVnc;
    private bool SpiceEnabled { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var client = await adminService[ClusterName].GetPveClientAsync();
        DefaultWebConsoleType = await client.GetDefaultWebConsoleAsync();
        await RefreshDataAsync();
    }

    private async Task OpenWebConsole(bool xTermJs)
        => await browserService.OpenPveConsole(adminService[ClusterName].GetUrlWebConsole(Vm.Node, Vm.VmType, Vm.VmId, Vm.Name, xTermJs));

    private static async Task OpenSpiceConsole() => await Task.CompletedTask;

    private async Task ChageStatus(VmStatus status)
    {
        if (status != VmStatus.Start)
        {
            if (!await dialogService.ConfirmAsync(L["Are you sure?"], L["Change status to {0} for {1}", status, Vm.VmId], true))
            {
                return;
            }
        }

        if (ChangeStatus.HasDelegate) { await ChangeStatus.InvokeAsync(status); }

        await commandExecutor.ExecuteAsync(new VmChangeStateCommand(ClusterName, Vm.VmId, status));
    }

    private async Task OnClickPower(RadzenSplitButtonItem item)
        => await ChageStatus(item == null
                                ? VmStatus.Shutdown
                                : Enum.Parse<VmStatus>(item.Value!));

    private async Task OnClickUtilityMenu(IToolBarUtility<IClusterResourceVm> item)
    {
        var execute = true;
        if (item.RequireConfirm)
        {
            execute = await dialogService.ConfirmAsync(L["Are you sure?"],
                                                       L["Execute {0} vm {1}", item.Text, Vm.VmId],
                                                       true);
        }

        if (execute) { await item.ExecuteAsync(ClusterName, Vm); }
    }

    private async Task OnClickOpenConsole(RadzenSplitButtonItem item)
        => await OpenConsole(Enum.TryParse<WebConsoleType>(item?.Value, out var parsedType)
                                ? parsedType
                                : WebConsoleType.NoVnc);

    private async Task OpenConsole(WebConsoleType type)
        => await (type switch
        {
            WebConsoleType.NoVnc => OpenWebConsole(false),
            WebConsoleType.XtermJs => OpenWebConsole(true),
            WebConsoleType.Spice => OpenSpiceConsole(),
            _ => OpenConsole(DefaultWebConsoleType)
        });

    public async Task RefreshDataAsync()
    {
        SpiceEnabled = false;
        await Task.CompletedTask;
    }
}
