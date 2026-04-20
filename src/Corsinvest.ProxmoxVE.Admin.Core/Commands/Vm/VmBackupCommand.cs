/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Commands.Base;
using Corsinvest.ProxmoxVE.Admin.Core.Commands.Models;
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth;
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth.Permissions;

namespace Corsinvest.ProxmoxVE.Admin.Core.Commands.Vm;

public record VmBackupCommand(string ClusterName,
                              long VmId,
                              string? Storage = null,
                              string? Mode = null,
                              string? Compress = null,
                              bool? Protected = null,
                              string? NotesTemplate = null,
                              int? BwLimit = null) : ICommand<PveTaskResult>
{
    public Permission RequiredPermission => ClusterPermissions.Vm.Backup;
    public string Context => $"Cluster {ClusterName}, VmId {VmId}, Storage {Storage}, Mode {Mode}";
    public Task<bool> HasPermissionAsync(IPermissionService permissionService)
        => permissionService.HasVmAsync(ClusterName, RequiredPermission, VmId);
}

public class VmBackupCommandHandler(IServiceProvider serviceProvider) : PveCommandHandlerBase<VmBackupCommand>(serviceProvider)
{
    protected override async Task<PveTaskResult> ExecuteAsync(VmBackupCommand command, CancellationToken cancellationToken)
    {
        var vm = await GetVmAsync(command.ClusterName, command.VmId);
        var client = await GetPveClientAsync(command.ClusterName);

        var result = await client.Nodes[vm.Node].Vzdump.Vzdump(bwlimit: command.BwLimit,
                                                                compress: command.Compress,
                                                                mode: command.Mode,
                                                                notes_template: command.NotesTemplate,
                                                                protected_: command.Protected,
                                                                storage: command.Storage,
                                                                vmid: command.VmId.ToString());

        return PveTaskResult.Success(result, command.ClusterName);
    }
}
