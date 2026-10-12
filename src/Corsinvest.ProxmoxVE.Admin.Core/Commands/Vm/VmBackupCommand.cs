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

// The backups of a guest are managed with the permission on the guest, creation and deletion alike:
// who may back it up may delete its backups, without the permission on the whole storage
public record VmDeleteBackupCommand(string ClusterName,
                                    long VmId,
                                    string Storage,
                                    string VolId) : ICommand<PveTaskResult>
{
    public Permission RequiredPermission => ClusterPermissions.Vm.Backup;
    public string Context => $"Cluster {ClusterName}, VmId {VmId}, Storage {Storage}, VolId {VolId}";
    public Task<bool> HasPermissionAsync(IPermissionService permissionService)
        => permissionService.HasVmAsync(ClusterName, RequiredPermission, VmId);
}

public class VmDeleteBackupCommandHandler(IServiceProvider serviceProvider) : PveCommandHandlerBase<VmDeleteBackupCommand>(serviceProvider)
{
    protected override async Task<PveTaskResult> ExecuteAsync(VmDeleteBackupCommand command, CancellationToken cancellationToken)
    {
        var vm = await GetVmAsync(command.ClusterName, command.VmId);
        var client = await GetPveClientAsync(command.ClusterName);
        var result = await client.Nodes[vm.Node].Storage[command.Storage].Content[command.VolId].Delete();
        return PveTaskResult.Success(result, command.ClusterName);
    }
}

public record VmUpdateBackupCommand(string ClusterName,
                                    long VmId,
                                    string Storage,
                                    string VolId,
                                    string? Notes,
                                    bool Protected) : ICommand<PveTaskResult>
{
    public Permission RequiredPermission => ClusterPermissions.Vm.Backup;
    public string Context => $"Cluster {ClusterName}, VmId {VmId}, Storage {Storage}, VolId {VolId}, Protected {Protected}";
    public Task<bool> HasPermissionAsync(IPermissionService permissionService)
        => permissionService.HasVmAsync(ClusterName, RequiredPermission, VmId);
}

public class VmUpdateBackupCommandHandler(IServiceProvider serviceProvider) : PveCommandHandlerBase<VmUpdateBackupCommand>(serviceProvider)
{
    protected override async Task<PveTaskResult> ExecuteAsync(VmUpdateBackupCommand command, CancellationToken cancellationToken)
    {
        var vm = await GetVmAsync(command.ClusterName, command.VmId);
        var client = await GetPveClientAsync(command.ClusterName);
        var result = await client.Nodes[vm.Node]
                                 .Storage[command.Storage]
                                 .Content[command.VolId]
                                 .Updateattributes(notes: command.Notes, protected_: command.Protected);
        return PveTaskResult.Success(result, command.ClusterName);
    }
}
