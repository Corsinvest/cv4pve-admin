/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Commands.Models;
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth;
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth.Permissions;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;

namespace Corsinvest.ProxmoxVE.Admin.Core.Commands.Base;

public abstract class PveCommandHandlerBase<TCommand>(IServiceProvider serviceProvider) : ICommandHandler<TCommand, PveTaskResult>
    where TCommand : ICommand<PveTaskResult>
{
    protected IAdminService AdminService { get; } = serviceProvider.GetRequiredService<IAdminService>();
    protected IAuditService AuditService { get; } = serviceProvider.GetRequiredService<IAuditService>();
    protected IPermissionService PermissionService { get; } = serviceProvider.GetRequiredService<IPermissionService>();

    public async Task<PveTaskResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default)
    {
        var action = typeof(TCommand).Name.Replace("Command", "");

        try
        {
            if (!await command.HasPermissionAsync(PermissionService))
            {
                await AuditService.LogAsync(action, false, $"Permission denied. {command.Context}");
                return PveTaskResult.Forbidden(command.ClusterName);
            }

            var result = await ExecuteAsync(command, cancellationToken);

            await AuditService.LogAsync(action,
                                        result.IsSuccess,
                                        $"{(result.IsSuccess ? "Success" : "Failed")}. {command.Context}");

            return result;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            await AuditService.LogAsync(action, false, $"Exception: {ex.Message}. {command.Context}");
            return PveTaskResult.Failure(command.ClusterName, ex.Message);
        }
    }

    protected abstract Task<PveTaskResult> ExecuteAsync(TCommand command, CancellationToken cancellationToken);

    protected async Task<IClusterResourceVm> GetVmAsync(string clusterName, long vmId)
        => (await AdminService[clusterName].CachedData.GetResourcesAsync(false))
                    .FirstOrDefault(a => a.ResourceType == ClusterResourceType.Vm && a.VmId == vmId)
                    ?? throw new InvalidOperationException($"VM/CT with ID {vmId} not found in cluster {clusterName}");

    protected Task<PveClient> GetPveClientAsync(string clusterName)
        => AdminService[clusterName].GetPveClientAsync();
}
