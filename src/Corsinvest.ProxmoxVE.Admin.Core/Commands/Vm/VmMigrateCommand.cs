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

public record VmMigrateCommand(string ClusterName,
                               long VmId,
                               string TargetNode,
                               bool Online = false,
                               string? TargetStorage = null,
                               int? BwLimit = null) : ICommand<PveTaskResult>
{
    public Permission RequiredPermission => ClusterPermissions.Vm.Migrate;
    public string Context => $"Cluster {ClusterName}, VmId {VmId}, TargetNode {TargetNode}, Online {Online}";
    public Task<bool> HasPermissionAsync(IPermissionService permissionService)
        => permissionService.HasVmAsync(ClusterName, RequiredPermission, VmId);
}

public class VmMigrateCommandHandler(IServiceProvider serviceProvider) : PveCommandHandlerBase<VmMigrateCommand>(serviceProvider)
{
    protected override async Task<PveTaskResult> ExecuteAsync(VmMigrateCommand command, CancellationToken cancellationToken)
    {
        var vm = await GetVmAsync(command.ClusterName, command.VmId);
        var client = await GetPveClientAsync(command.ClusterName);
        var result = vm.VmType switch
        {
            VmType.Qemu => await client.Nodes[vm.Node].Qemu[vm.VmId].Migrate.MigrateVm(target: command.TargetNode,
                                                                                       online: command.Online,
                                                                                       targetstorage: command.TargetStorage,
                                                                                       bwlimit: command.BwLimit),

            VmType.Lxc => await client.Nodes[vm.Node].Lxc[vm.VmId].Migrate.MigrateVm(target: command.TargetNode,
                                                                                     online: command.Online,
                                                                                     target_storage: command.TargetStorage,
                                                                                     bwlimit: command.BwLimit),
            _ => throw new InvalidEnumArgumentException(),
        };
        return PveTaskResult.Success(result, command.ClusterName);
    }
}
