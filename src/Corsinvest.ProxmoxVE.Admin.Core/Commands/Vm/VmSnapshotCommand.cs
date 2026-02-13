/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Commands.Base;
using Corsinvest.ProxmoxVE.Admin.Core.Commands.Models;
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;

namespace Corsinvest.ProxmoxVE.Admin.Core.Commands.Vm;

public record VmCreateSnapshotCommand(
    string ClusterName,
    long VmId,
    string Name,
    string? Description = null,
    bool IncludeVmState = false) : ICommand<PveTaskResult>;

public class VmCreateSnapshotCommandHandler(IAdminService adminService, IAuditService auditService)
    : PveCommandHandlerBase<VmCreateSnapshotCommand>(adminService, auditService)
{
    public override async Task<PveTaskResult> HandleAsync(VmCreateSnapshotCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var vm = await GetVmAsync(command.ClusterName, command.VmId);
            var client = await GetPveClientAsync(command.ClusterName);
            var result = vm.VmType switch
            {
                VmType.Qemu => await client.Nodes[vm.Node].Qemu[vm.VmId].Snapshot.Snapshot(command.Name, command.Description, command.IncludeVmState),
                VmType.Lxc => await client.Nodes[vm.Node].Lxc[vm.VmId].Snapshot.Snapshot(command.Name, command.Description),
                _ => throw new InvalidEnumArgumentException(),
            };

            await LogAuditAsync("VmCreateSnapshot",
                                true,
                                $"Create snapshot Cluster {command.ClusterName}, VmId {command.VmId}, Name {command.Name}");

            return PveTaskResult.Success(result, command.ClusterName);
        }
        catch (Exception ex)
        {
            return PveTaskResult.Failure(command.ClusterName, $"Failed to create snapshot for VM {command.VmId}: {ex.Message}");
        }
    }
}

public record VmRemoveSnapshotCommand(
    string ClusterName,
    long VmId,
    string SnapshotName,
    bool Force = false
) : ICommand<PveTaskResult>;

public class VmRemoveSnapshotCommandHandler(IAdminService adminService, IAuditService auditService)
    : PveCommandHandlerBase<VmRemoveSnapshotCommand>(adminService, auditService)
{
    public override async Task<PveTaskResult> HandleAsync(VmRemoveSnapshotCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var vm = await GetVmAsync(command.ClusterName, command.VmId);
            var client = await GetPveClientAsync(command.ClusterName);
            var result = vm.VmType switch
            {
                VmType.Qemu => await client.Nodes[vm.Node].Qemu[vm.VmId].Snapshot[command.SnapshotName].Delsnapshot(command.Force),
                VmType.Lxc => await client.Nodes[vm.Node].Lxc[vm.VmId].Snapshot[command.SnapshotName].Delsnapshot(command.Force),
                _ => throw new InvalidEnumArgumentException(),
            };

            await LogAuditAsync("VmRemoveSnapshot",
                              true,
                              $"Remove snapshot Cluster {command.ClusterName}, VmId {command.VmId}, Snapshot {command.SnapshotName}");

            return PveTaskResult.Success(result, command.ClusterName);
        }
        catch (Exception ex)
        {
            return PveTaskResult.Failure(command.ClusterName, $"Failed to remove snapshot {command.SnapshotName} for VM {command.VmId}: {ex.Message}");
        }
    }
}

public record VmUpdateSnapshotCommand(
    string ClusterName,
    long VmId,
    string SnapshotName,
    string? Description = null
) : ICommand<PveTaskResult>;

public class VmUpdateSnapshotCommandHandler(IAdminService adminService, IAuditService auditService)
    : PveCommandHandlerBase<VmUpdateSnapshotCommand>(adminService, auditService)
{
    public override async Task<PveTaskResult> HandleAsync(VmUpdateSnapshotCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var vm = await GetVmAsync(command.ClusterName, command.VmId);
            var client = await GetPveClientAsync(command.ClusterName);
            var result = vm.VmType switch
            {
                VmType.Qemu => await client.Nodes[vm.Node].Qemu[vm.VmId].Snapshot[command.SnapshotName].Config.UpdateSnapshotConfig(command.Description),
                VmType.Lxc => await client.Nodes[vm.Node].Lxc[vm.VmId].Snapshot[command.SnapshotName].Config.UpdateSnapshotConfig(command.Description),
                _ => throw new InvalidEnumArgumentException(),
            };

            await LogAuditAsync("VmUpdateSnapshot",
                              true,
                              $"Update snapshot Cluster {command.ClusterName}, VmId {command.VmId}, Snapshot {command.SnapshotName}");

            return PveTaskResult.Success(result, command.ClusterName);
        }
        catch (Exception ex)
        {
            return PveTaskResult.Failure(command.ClusterName, $"Failed to update snapshot {command.SnapshotName} for VM {command.VmId}: {ex.Message}");
        }
    }
}

public record VmRollbackSnapshotCommand(
    string ClusterName,
    long VmId,
    string SnapshotName
) : ICommand<PveTaskResult>;

public class VmRollbackSnapshotCommandHandler(IAdminService adminService, IAuditService auditService)
    : PveCommandHandlerBase<VmRollbackSnapshotCommand>(adminService, auditService)
{
    public override async Task<PveTaskResult> HandleAsync(VmRollbackSnapshotCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var vm = await GetVmAsync(command.ClusterName, command.VmId);
            var client = await GetPveClientAsync(command.ClusterName);
            var result = vm.VmType switch
            {
                VmType.Qemu => await client.Nodes[vm.Node].Qemu[vm.VmId].Snapshot[command.SnapshotName].Rollback.Rollback(),
                VmType.Lxc => await client.Nodes[vm.Node].Lxc[vm.VmId].Snapshot[command.SnapshotName].Rollback.Rollback(),
                _ => throw new InvalidEnumArgumentException(),
            };

            await LogAuditAsync("VmRollbackSnapshot",
                              true,
                              $"Rollback snapshot Cluster {command.ClusterName}, VmId {command.VmId}, Snapshot {command.SnapshotName}");

            return PveTaskResult.Success(result, command.ClusterName);
        }
        catch (Exception ex)
        {
            return PveTaskResult.Failure(command.ClusterName, $"Failed to rollback snapshot {command.SnapshotName} for VM {command.VmId}: {ex.Message}");
        }
    }
}
