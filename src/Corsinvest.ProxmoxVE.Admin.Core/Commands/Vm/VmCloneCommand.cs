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

public record VmCloneCommand(string ClusterName,
                             long VmId,
                             long NewVmId,
                             string? NewName = null,
                             string? TargetNode = null,
                             string? TargetStorage = null,
                             bool FullClone = true,
                             string? Description = null) : ICommand<PveTaskResult>
{
    public Permission RequiredPermission => ClusterPermissions.Vm.Clone;
    public string Context => $"Cluster {ClusterName}, VmId {VmId}, NewVmId {NewVmId}";
    public Task<bool> HasPermissionAsync(IPermissionService permissionService)
        => permissionService.HasVmAsync(ClusterName, RequiredPermission, VmId);
}

public class VmCloneCommandHandler(IServiceProvider serviceProvider) : PveCommandHandlerBase<VmCloneCommand>(serviceProvider)
{
    protected override async Task<PveTaskResult> ExecuteAsync(VmCloneCommand command, CancellationToken cancellationToken)
    {
        var vm = await GetVmAsync(command.ClusterName, command.VmId);
        var client = await GetPveClientAsync(command.ClusterName);
        var result = vm.VmType switch
        {
            VmType.Qemu => await client.Nodes[vm.Node].Qemu[vm.VmId].Clone.CloneVm(newid: (int)command.NewVmId,
                                                                                   description: command.Description,
                                                                                   full: command.FullClone,
                                                                                   name: command.NewName,
                                                                                   storage: command.TargetStorage,
                                                                                   target: command.TargetNode),

            VmType.Lxc => await client.Nodes[vm.Node].Lxc[vm.VmId].Clone.CloneVm(newid: (int)command.NewVmId,
                                                                                 description: command.Description,
                                                                                 full: command.FullClone,
                                                                                 hostname: command.NewName,
                                                                                 storage: command.TargetStorage,
                                                                                 target: command.TargetNode),
            _ => throw new InvalidEnumArgumentException(),
        };
        return PveTaskResult.Success(result, command.ClusterName);
    }
}
