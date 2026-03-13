/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Commands.Base;
using Corsinvest.ProxmoxVE.Admin.Core.Commands.Models;
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth;

namespace Corsinvest.ProxmoxVE.Admin.Core.Commands.Storage;

public record StorageDeleteContentCommand(
    string ClusterName,
    string Node,
    string Storage,
    string VolId
) : ICommand<PveTaskResult>;

public class StorageDeleteContentCommandHandler(IAdminService adminService, IAuditService auditService)
    : PveCommandHandlerBase<StorageDeleteContentCommand>(adminService, auditService)
{
    public override async Task<PveTaskResult> HandleAsync(StorageDeleteContentCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = await GetPveClientAsync(command.ClusterName);
            var result = await client.Nodes[command.Node].Storage[command.Storage].Content[command.VolId].Delete();

            await LogAuditAsync("StorageDeleteContent",
                                true,
                                $"Delete content Cluster {command.ClusterName}, Node {command.Node}, Storage {command.Storage}, VolId {command.VolId}");

            return PveTaskResult.Success(result, command.ClusterName);
        }
        catch (Exception ex)
        {
            return PveTaskResult.Failure(command.ClusterName, $"Failed to delete content {command.VolId}: {ex.Message}");
        }
    }
}
