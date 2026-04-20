/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Commands.Base;
using Corsinvest.ProxmoxVE.Admin.Core.Commands.Models;
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth;
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth.Permissions;

namespace Corsinvest.ProxmoxVE.Admin.Core.Commands.Storage;

public record StorageDeleteContentCommand(string ClusterName,
                                          string Node,
                                          string Storage,
                                          string VolId) : ICommand<PveTaskResult>
{
    public Permission RequiredPermission => ClusterPermissions.Storage.DeleteContent;
    public string Context => $"Cluster {ClusterName}, Node {Node}, Storage {Storage}, VolId {VolId}";
    public Task<bool> HasPermissionAsync(IPermissionService permissionService)
        => permissionService.HasAsync(ClusterName, RequiredPermission.Key, PermissionHelper.GetPathStorage(Storage));
}

public class StorageDeleteContentCommandHandler(IServiceProvider serviceProvider)
    : PveCommandHandlerBase<StorageDeleteContentCommand>(serviceProvider)
{
    protected override async Task<PveTaskResult> ExecuteAsync(StorageDeleteContentCommand command, CancellationToken cancellationToken)
    {
        var client = await GetPveClientAsync(command.ClusterName);
        var result = await client.Nodes[command.Node].Storage[command.Storage].Content[command.VolId].Delete();
        return PveTaskResult.Success(result, command.ClusterName);
    }
}
