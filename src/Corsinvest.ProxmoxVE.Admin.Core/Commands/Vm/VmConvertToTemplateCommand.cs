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

public record VmConvertToTemplateCommand(string ClusterName, long VmId) : ICommand<PveTaskResult>
{
    public Permission RequiredPermission => ClusterPermissions.Vm.Template;
    public string Context => $"Cluster {ClusterName}, VmId {VmId}";
    public Task<bool> HasPermissionAsync(IPermissionService permissionService)
        => permissionService.HasVmAsync(ClusterName, RequiredPermission, VmId);
}

public class VmConvertToTemplateCommandHandler(IServiceProvider serviceProvider) : PveCommandHandlerBase<VmConvertToTemplateCommand>(serviceProvider)
{
    protected override async Task<PveTaskResult> ExecuteAsync(VmConvertToTemplateCommand command, CancellationToken cancellationToken)
    {
        var vm = await GetVmAsync(command.ClusterName, command.VmId);
        var client = await GetPveClientAsync(command.ClusterName);
        var result = vm.VmType switch
        {
            VmType.Qemu => await client.Nodes[vm.Node].Qemu[vm.VmId].Template.Template(),
            VmType.Lxc => await client.Nodes[vm.Node].Lxc[vm.VmId].Template.Template(),
            _ => throw new InvalidEnumArgumentException(),
        };
        return PveTaskResult.Success(result, command.ClusterName);
    }
}
