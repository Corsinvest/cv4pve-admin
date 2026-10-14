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

public record VmResizeDiskCommand(string ClusterName,
                                  long VmId,
                                  string Disk,
                                  string Size) : ICommand<PveTaskResult>
{
    public Permission RequiredPermission => ClusterPermissions.Vm.Resize;
    public string Context => $"Cluster {ClusterName}, VmId {VmId}, Disk {Disk}, Size {Size}";
    public Task<bool> HasPermissionAsync(IPermissionService permissionService)
        => permissionService.HasVmAsync(ClusterName, RequiredPermission, VmId);
}

public class VmResizeDiskCommandHandler(IServiceProvider serviceProvider) : PveCommandHandlerBase<VmResizeDiskCommand>(serviceProvider)
{
    protected override async Task<PveTaskResult> ExecuteAsync(VmResizeDiskCommand command, CancellationToken cancellationToken)
    {
        var vm = await GetVmAsync(command.ClusterName, command.VmId);
        var client = await GetPveClientAsync(command.ClusterName);
        var result = vm.VmType switch
        {
            VmType.Qemu => await client.Nodes[vm.Node].Qemu[vm.VmId].Resize.ResizeVm(command.Disk, command.Size),
            VmType.Lxc => await client.Nodes[vm.Node].Lxc[vm.VmId].Resize.ResizeVm(command.Disk, command.Size),
            _ => throw new InvalidEnumArgumentException(),
        };
        return PveTaskResult.Success(result, command.ClusterName);
    }
}
