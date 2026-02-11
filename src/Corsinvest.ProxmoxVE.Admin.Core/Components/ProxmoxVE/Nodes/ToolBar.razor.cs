using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Common;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components.ProxmoxVE.Nodes;

public partial class ToolBar(IBrowserService browserService,
                             IAdminService adminService,
                             DialogService dialogService,
                             ContextMenuService contextMenuService,
                             IEnumerable<IToolBarUtility<IClusterResourceNode>>  Utility) : IRefreshableData, IClusterName
{
    [EditorRequired, Parameter] public IClusterResourceNode Node { get; set; } = default!;
    [EditorRequired, Parameter] public string ClusterName { get; set; } = default!;
    [Parameter] public bool CanConsole { get; set; }
    [Parameter] public bool CanChangeStatus { get; set; }
    [Parameter] public bool OnlyIcon { get; set; }
    [Parameter] public EventCallback<string> ChangeStatus { get; set; }

    private WebConsoleType DefaultWebConsoleType { get; set; } = WebConsoleType.NoVnc;

    private bool SpiceEnabled { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var client = await adminService[ClusterName].GetPveClientAsync();
        DefaultWebConsoleType = await client.GetDefaultWebConsoleAsync();

        await RefreshDataAsync();
    }

    private async Task OpenWebConsole(bool xTermJs)
        => await browserService.OpenPveConsole(adminService[ClusterName].GetUrlWebConsole(Node.Node, xTermJs));

    private static async Task OpenSpiceConsole() => await Task.CompletedTask;

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

    private async Task RebootAsync()
    {
        if (await dialogService.ConfirmAsync(L["Are you sure?"], L["Reboot node {0}", Node.Node], true))
        {
            await ChangeStatusExecute("reboot");
        }
    }

    private async Task ShutdownAsync()
    {
        if (await dialogService.ConfirmAsync(L["Are you sure?"], L["Shutdown node {0}", Node.Node], true))
        {
            await ChangeStatusExecute("shutdown");
        }
    }

    private async Task OnClickUtilityMenu(IToolBarUtility<IClusterResourceNode> item)
    {
        var execute = true;
        if (item.RequireConfirm)
        {
            execute = await dialogService.ConfirmAsync(L["Are you sure?"],
                                                       L["Execute {0} node {1}", item.Text, Node.Node],
                                                       true);
        }

        if (execute) { await item.ExecuteAsync(ClusterName, Node); }
    }

    private async Task ChangeStatusExecute(string status)
    {
        if (ChangeStatus.HasDelegate) { await ChangeStatus.InvokeAsync(status); }

        var client = await adminService[ClusterName].GetPveClientAsync();
        await client.Nodes[Node].Status.NodeCmd(status);
    }

    private async Task OnClickPower(RadzenSplitButtonItem item)
    {
        switch (item?.Value)
        {
            case null: await ShutdownAsync(); break;
            case "reboot": await RebootAsync(); break;
            default: break;
        }
    }

    public async Task RefreshDataAsync()
    {
        SpiceEnabled = false;
        await Task.CompletedTask;
    }
}
