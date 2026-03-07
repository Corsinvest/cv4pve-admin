/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Commands.Base;
using Corsinvest.ProxmoxVE.Admin.Core.Commands.Models;
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth;

namespace Corsinvest.ProxmoxVE.Admin.Core.Commands.Vm;

public record VmBackupCommand(
    string ClusterName,
    long VmId,
    string? Storage = null,
    string? Mode = null,
    string? Compress = null,
    bool? Protected = null,
    string? NotesTemplate = null,
    int? BwLimit = null
) : ICommand<PveTaskResult>;

public class VmBackupCommandHandler(IAdminService adminService, IAuditService auditService)
    : PveCommandHandlerBase<VmBackupCommand>(adminService, auditService)
{
    public override async Task<PveTaskResult> HandleAsync(VmBackupCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var vm = await GetVmAsync(command.ClusterName, command.VmId);
            var client = await GetPveClientAsync(command.ClusterName);

            var result = await client.Nodes[vm.Node].Vzdump.Vzdump(
                bwlimit: command.BwLimit,
                compress: command.Compress,
                mode: command.Mode,
                notes_template: command.NotesTemplate,
                protected_: command.Protected,
                storage: command.Storage,
                vmid: command.VmId.ToString());

            await LogAuditAsync("VmBackup",
                              true,
                              $"Backup VM Cluster {command.ClusterName}, VmId {command.VmId}, Storage {command.Storage}, Mode {command.Mode}");

            return PveTaskResult.Success(result, command.ClusterName);
        }
        catch (Exception ex)
        {
            return PveTaskResult.Failure(command.ClusterName, $"Failed to backup VM {command.VmId}: {ex.Message}");
        }
    }
}
