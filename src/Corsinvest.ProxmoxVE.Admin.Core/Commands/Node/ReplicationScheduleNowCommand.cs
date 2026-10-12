/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Commands.Base;
using Corsinvest.ProxmoxVE.Admin.Core.Commands.Models;
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth;
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth.Permissions;

namespace Corsinvest.ProxmoxVE.Admin.Core.Commands.Node;

// VmId: set when the job is run from the page of a guest, where the permission is the one on the guest.
// Null from the page of a node, where it is the one on the node.
public record ReplicationScheduleNowCommand(string ClusterName,
                                            string Node,
                                            string JobId,
                                            long? VmId = null) : ICommand<PveTaskResult>
{
    public Permission RequiredPermission
        => VmId.HasValue
            ? ClusterPermissions.Vm.ReplicationScheduleNow
            : ClusterPermissions.Node.ReplicationScheduleNow;

    public string Context => $"Cluster {ClusterName}, Node {Node}, Job {JobId}";

    public Task<bool> HasPermissionAsync(IPermissionService permissionService)
        => VmId.HasValue
            ? permissionService.HasVmAsync(ClusterName, RequiredPermission, VmId.Value)
            : permissionService.HasNodeAsync(ClusterName, RequiredPermission, Node);
}

public class ReplicationScheduleNowCommandHandler(IServiceProvider serviceProvider)
    : PveCommandHandlerBase<ReplicationScheduleNowCommand>(serviceProvider)
{
    protected override async Task<PveTaskResult> ExecuteAsync(ReplicationScheduleNowCommand command, CancellationToken cancellationToken)
    {
        var client = await GetPveClientAsync(command.ClusterName);
        var result = await client.Nodes[command.Node].Replication[command.JobId].ScheduleNow.ScheduleNow();
        return PveTaskResult.Success(result, command.ClusterName);
    }
}
