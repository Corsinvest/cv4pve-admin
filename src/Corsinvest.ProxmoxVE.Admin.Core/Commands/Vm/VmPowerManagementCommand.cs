/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Commands.Base;
using Corsinvest.ProxmoxVE.Admin.Core.Commands.Models;
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth;
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth.Permissions;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;

namespace Corsinvest.ProxmoxVE.Admin.Core.Commands.Vm;

public record VmPowerManagementCommand(string ClusterName, long VmId, VmStatus Action) : ICommand<PveTaskResult>
{
    public Permission RequiredPermission => ClusterPermissions.Vm.PowerManagement;
    public string Context => $"Cluster {ClusterName}, VmId {VmId}, Action {Action}";
    public Task<bool> HasPermissionAsync(IPermissionService permissionService)
        => permissionService.HasVmAsync(ClusterName, RequiredPermission, VmId);
}

public class VmPowerManagementCommandHandler(IServiceProvider serviceProvider) : PveCommandHandlerBase<VmPowerManagementCommand>(serviceProvider)
{
    protected override async Task<PveTaskResult> ExecuteAsync(VmPowerManagementCommand command, CancellationToken cancellationToken)
    {
        var vm = await GetVmAsync(command.ClusterName, command.VmId);
        var result = await VmHelper.ChangeStatusVmAsync(await GetPveClientAsync(command.ClusterName),
                                                         vm.Node,
                                                         vm.VmType,
                                                         vm.VmId,
                                                         command.Action);
        return PveTaskResult.Success(result, command.ClusterName);
    }
}
