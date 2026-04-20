/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Commands.Base;
using Corsinvest.ProxmoxVE.Admin.Core.Commands.Models;
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth;
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth.Permissions;

namespace Corsinvest.ProxmoxVE.Admin.Core.Commands.Node;

public enum NodePowerAction
{
    Reboot,
    Shutdown
}

public record NodePowerManagementCommand(string ClusterName, string Node, NodePowerAction Action)
    : ICommand<PveTaskResult>
{
    public Permission RequiredPermission => ClusterPermissions.Node.PowerManagement;
    public string Context => $"Cluster {ClusterName}, Node {Node}, Action {Action}";
    public Task<bool> HasPermissionAsync(IPermissionService permissionService)
        => permissionService.HasNodeAsync(ClusterName, RequiredPermission, Node);
}

public class NodePowerManagementCommandHandler(IServiceProvider serviceProvider)
    : PveCommandHandlerBase<NodePowerManagementCommand>(serviceProvider)
{
    protected override async Task<PveTaskResult> ExecuteAsync(NodePowerManagementCommand command, CancellationToken cancellationToken)
    {
        var client = await GetPveClientAsync(command.ClusterName);
        var statusArg = command.Action switch
        {
            NodePowerAction.Reboot => "reboot",
            NodePowerAction.Shutdown => "shutdown",
            _ => throw new InvalidEnumArgumentException()
        };

        var result = await client.Nodes[command.Node].Status.NodeCmd(statusArg);
        return PveTaskResult.Success(result, command.ClusterName);
    }
}
