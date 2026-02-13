/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Commands.Base;
using Corsinvest.ProxmoxVE.Admin.Core.Commands.Models;
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;

namespace Corsinvest.ProxmoxVE.Admin.Core.Commands.Vm;

public record VmMigrateCommand(
    string ClusterName,
    long VmId,
    string TargetNode,
    bool Online = false,
    string? TargetStorage = null,
    float? BwLimit = null
) : ICommand<PveTaskResult>;

public class VmMigrateCommandHandler(IAdminService adminService, IAuditService auditService)
    : PveCommandHandlerBase<VmMigrateCommand>(adminService, auditService)
{
    public override async Task<PveTaskResult> HandleAsync(VmMigrateCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var vm = await GetVmAsync(command.ClusterName, command.VmId);
            var client = await GetPveClientAsync(command.ClusterName);
            var result = vm.VmType switch
            {
                VmType.Qemu => await client.Nodes[vm.Node].Qemu[vm.VmId].Migrate.MigrateVm(
                    target: command.TargetNode,
                    online: command.Online,
                    targetstorage: command.TargetStorage,
                    bwlimit: command.BwLimit.HasValue ? (int)command.BwLimit.Value : null),
                VmType.Lxc => await client.Nodes[vm.Node].Lxc[vm.VmId].Migrate.MigrateVm(
                    target: command.TargetNode,
                    online: command.Online,
                    target_storage: command.TargetStorage,
                    bwlimit: command.BwLimit),
                _ => throw new InvalidEnumArgumentException(),
            };

            await LogAuditAsync("VmMigrate",
                              true,
                              $"Migrate VM Cluster {command.ClusterName}, VmId {command.VmId}, TargetNode {command.TargetNode}, Online {command.Online}");

            return PveTaskResult.Success(result, command.ClusterName);
        }
        catch (Exception ex)
        {
            return PveTaskResult.Failure(command.ClusterName, $"Failed to migrate VM {command.VmId} to {command.TargetNode}: {ex.Message}");
        }
    }
}
