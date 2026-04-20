/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Commands;
using Corsinvest.ProxmoxVE.Admin.Core.Commands.Node;
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Common;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components.ProxmoxVE.Nodes;

public partial class ToolBar(IBrowserService browserService,
                             IAdminService adminService,
                             DialogService dialogService,
                             ContextMenuService contextMenuService,
                             IUiCommandExecutor uiExecutor,
                             IEnumerable<IToolBarUtility<IClusterResourceNode>> Utility) : IRefreshableData, IClusterName
{
    [EditorRequired, Parameter] public IClusterResourceNode Node { get; set; } = default!;
    [EditorRequired, Parameter] public string ClusterName { get; set; } = default!;
    [Parameter] public bool OnlyIcon { get; set; }
    [Parameter] public EventCallback<NodePowerAction> ChangeStatus { get; set; }

    private WebConsoleType DefaultWebConsoleType { get; set; } = WebConsoleType.NoVnc;

    private bool CanConsole { get; set; }
    private bool CanChangeStatus { get; set; }
    private bool SpiceEnabled { get; set; }
    private bool IsPam => adminService[ClusterName].Settings.WebApi.IsPam;

    protected override async Task OnInitializedAsync()
    {
        var client = await adminService[ClusterName].GetPveClientAsync();
        DefaultWebConsoleType = await client.GetDefaultWebConsoleAsync();

        CanConsole = await PermissionService.HasNodeAsync(ClusterName, ClusterPermissions.Node.Console, Node.Node);
        CanChangeStatus = await PermissionService.HasNodeAsync(ClusterName, ClusterPermissions.Node.PowerManagement, Node.Node);
        await RefreshDataAsync();
    }

    private Task OpenWebConsole(bool xTermJs)
        => browserService.OpenPveConsole(adminService[ClusterName].GetWebConsoleUrl(Node.Node, xTermJs));

    private static Task OpenSpiceConsole() => Task.CompletedTask;

    private Task OnClickOpenConsole(RadzenSplitButtonItem item)
         => OpenConsole(Enum.TryParse<WebConsoleType>(item?.Value, out var parsedType)
                                 ? parsedType
                                 : WebConsoleType.NoVnc);

    private Task OpenConsole(WebConsoleType type)
        => type switch
        {
            WebConsoleType.NoVnc => OpenWebConsole(false),
            WebConsoleType.XtermJs => OpenWebConsole(true),
            WebConsoleType.Spice => OpenSpiceConsole(),
            _ => OpenConsole(DefaultWebConsoleType)
        };

    private async Task RebootAsync()
    {
        if (await dialogService.ConfirmAsync(L["Are you sure?"], L["Reboot node {0}", Node.Node], true))
        {
            await ChangeStatusExecute(NodePowerAction.Reboot);
        }
    }

    private async Task ShutdownAsync()
    {
        if (await dialogService.ConfirmAsync(L["Are you sure?"], L["Shutdown node {0}", Node.Node], true))
        {
            await ChangeStatusExecute(NodePowerAction.Shutdown);
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

    private async Task ChangeStatusExecute(NodePowerAction action)
    {
        var result = await uiExecutor.ExecuteAndNotifyAsync(new NodePowerManagementCommand(ClusterName, Node.Node, action));
        if (result.IsSuccess && ChangeStatus.HasDelegate) { await ChangeStatus.InvokeAsync(action); }
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
