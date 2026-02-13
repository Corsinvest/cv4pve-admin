/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Commands.Models;
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;

namespace Corsinvest.ProxmoxVE.Admin.Core.Commands.Base;

public abstract class PveCommandHandlerBase<TCommand>(IAdminService adminService, IAuditService auditService)
    : ICommandHandler<TCommand, PveTaskResult>
    where TCommand : ICommand<PveTaskResult>
{
    public abstract Task<PveTaskResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default);

    protected async Task<IClusterResourceVm> GetVmAsync(string clusterName, long vmId)
        => (await adminService[clusterName].CachedData.GetResourcesAsync(false))
                    .FirstOrDefault(a => a.ResourceType == ClusterResourceType.Vm && a.VmId == vmId)
                    ?? throw new InvalidOperationException($"VM/CT with ID {vmId} not found in cluster {clusterName}");

    protected Task<PveClient> GetPveClientAsync(string clusterName)
        => adminService[clusterName].GetPveClientAsync();

    protected Task LogAuditAsync(string action, bool success, string message)
        => auditService.LogAsync(action, success, message);
}
