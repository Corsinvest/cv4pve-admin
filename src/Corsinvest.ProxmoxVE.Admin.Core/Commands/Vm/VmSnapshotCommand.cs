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

public record VmCreateSnapshotCommand(string ClusterName,
                                      long VmId,
                                      string Name,
                                      string? Description = null,
                                      bool IncludeVmState = false) : ICommand<PveTaskResult>
{
    public Permission RequiredPermission => ClusterPermissions.Vm.Snapshot;
    public string Context => $"Cluster {ClusterName}, VmId {VmId}, Name {Name}";
    public Task<bool> HasPermissionAsync(IPermissionService permissionService)
        => permissionService.HasVmAsync(ClusterName, RequiredPermission, VmId);
}

public class VmCreateSnapshotCommandHandler(IServiceProvider serviceProvider) : PveCommandHandlerBase<VmCreateSnapshotCommand>(serviceProvider)
{
    protected override async Task<PveTaskResult> ExecuteAsync(VmCreateSnapshotCommand command, CancellationToken cancellationToken)
    {
        var vm = await GetVmAsync(command.ClusterName, command.VmId);
        var client = await GetPveClientAsync(command.ClusterName);
        var result = vm.VmType switch
        {
            VmType.Qemu => await client.Nodes[vm.Node].Qemu[vm.VmId].Snapshot.Snapshot(command.Name, command.Description, command.IncludeVmState),
            VmType.Lxc => await client.Nodes[vm.Node].Lxc[vm.VmId].Snapshot.Snapshot(command.Name, command.Description),
            _ => throw new InvalidEnumArgumentException(),
        };
        return PveTaskResult.Success(result, command.ClusterName);
    }
}

public record VmRemoveSnapshotCommand(string ClusterName,
                                      long VmId,
                                      string SnapshotName,
                                      bool Force = false) : ICommand<PveTaskResult>
{
    public Permission RequiredPermission => ClusterPermissions.Vm.Snapshot;
    public string Context => $"Cluster {ClusterName}, VmId {VmId}, Snapshot {SnapshotName}";
    public Task<bool> HasPermissionAsync(IPermissionService permissionService)
        => permissionService.HasVmAsync(ClusterName, RequiredPermission, VmId);
}

public class VmRemoveSnapshotCommandHandler(IServiceProvider serviceProvider) : PveCommandHandlerBase<VmRemoveSnapshotCommand>(serviceProvider)
{
    protected override async Task<PveTaskResult> ExecuteAsync(VmRemoveSnapshotCommand command, CancellationToken cancellationToken)
    {
        var vm = await GetVmAsync(command.ClusterName, command.VmId);
        var client = await GetPveClientAsync(command.ClusterName);
        var result = vm.VmType switch
        {
            VmType.Qemu => await client.Nodes[vm.Node].Qemu[vm.VmId].Snapshot[command.SnapshotName].Delsnapshot(command.Force),
            VmType.Lxc => await client.Nodes[vm.Node].Lxc[vm.VmId].Snapshot[command.SnapshotName].Delsnapshot(command.Force),
            _ => throw new InvalidEnumArgumentException(),
        };
        return PveTaskResult.Success(result, command.ClusterName);
    }
}

public record VmUpdateSnapshotCommand(string ClusterName,
                                      long VmId,
                                      string SnapshotName,
                                      string? Description = null) : ICommand<PveTaskResult>
{
    public Permission RequiredPermission => ClusterPermissions.Vm.Snapshot;
    public string Context => $"Cluster {ClusterName}, VmId {VmId}, Snapshot {SnapshotName}";
    public Task<bool> HasPermissionAsync(IPermissionService permissionService)
        => permissionService.HasVmAsync(ClusterName, RequiredPermission, VmId);
}

public class VmUpdateSnapshotCommandHandler(IServiceProvider serviceProvider) : PveCommandHandlerBase<VmUpdateSnapshotCommand>(serviceProvider)
{
    protected override async Task<PveTaskResult> ExecuteAsync(VmUpdateSnapshotCommand command, CancellationToken cancellationToken)
    {
        var vm = await GetVmAsync(command.ClusterName, command.VmId);
        var client = await GetPveClientAsync(command.ClusterName);
        var result = vm.VmType switch
        {
            VmType.Qemu => await client.Nodes[vm.Node].Qemu[vm.VmId].Snapshot[command.SnapshotName].Config.UpdateSnapshotConfig(command.Description),
            VmType.Lxc => await client.Nodes[vm.Node].Lxc[vm.VmId].Snapshot[command.SnapshotName].Config.UpdateSnapshotConfig(command.Description),
            _ => throw new InvalidEnumArgumentException(),
        };
        return PveTaskResult.Success(result, command.ClusterName);
    }
}

public record VmRollbackSnapshotCommand(string ClusterName,
                                        long VmId,
                                        string SnapshotName) : ICommand<PveTaskResult>
{
    public Permission RequiredPermission => ClusterPermissions.Vm.SnapshotRallback;
    public string Context => $"Cluster {ClusterName}, VmId {VmId}, Snapshot {SnapshotName}";
    public Task<bool> HasPermissionAsync(IPermissionService permissionService)
        => permissionService.HasVmAsync(ClusterName, RequiredPermission, VmId);
}

public class VmRollbackSnapshotCommandHandler(IServiceProvider serviceProvider) : PveCommandHandlerBase<VmRollbackSnapshotCommand>(serviceProvider)
{
    protected override async Task<PveTaskResult> ExecuteAsync(VmRollbackSnapshotCommand command, CancellationToken cancellationToken)
    {
        var vm = await GetVmAsync(command.ClusterName, command.VmId);
        var client = await GetPveClientAsync(command.ClusterName);
        var result = vm.VmType switch
        {
            VmType.Qemu => await client.Nodes[vm.Node].Qemu[vm.VmId].Snapshot[command.SnapshotName].Rollback.Rollback(),
            VmType.Lxc => await client.Nodes[vm.Node].Lxc[vm.VmId].Snapshot[command.SnapshotName].Rollback.Rollback(),
            _ => throw new InvalidEnumArgumentException(),
        };
        return PveTaskResult.Success(result, command.ClusterName);
    }
}
