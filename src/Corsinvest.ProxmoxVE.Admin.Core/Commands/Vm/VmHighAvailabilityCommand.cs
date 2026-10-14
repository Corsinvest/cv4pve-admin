/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Commands.Base;
using Corsinvest.ProxmoxVE.Admin.Core.Commands.Models;
using Corsinvest.ProxmoxVE.Admin.Core.Helpers;
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth;
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth.Permissions;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;

namespace Corsinvest.ProxmoxVE.Admin.Core.Commands.Vm;

public enum VmHighAvailabilityAction
{
    Add,
    Remove,
    Migrate,
    Relocate
}

public record VmHighAvailabilityCommand(string ClusterName,
                                        long VmId,
                                        VmHighAvailabilityAction Action,
                                        string? Group = null,
                                        string? State = null,
                                        int? MaxRelocate = null,
                                        int? MaxRestart = null,
                                        string? TargetNode = null) : ICommand<PveTaskResult>
{
    public Permission RequiredPermission => ClusterPermissions.Vm.HighAvailability;
    public string Context => $"Cluster {ClusterName}, VmId {VmId}, Action {Action}";
    public Task<bool> HasPermissionAsync(IPermissionService permissionService)
        => permissionService.HasVmAsync(ClusterName, RequiredPermission, VmId);
}

public class VmHighAvailabilityCommandHandler(IServiceProvider serviceProvider) : PveCommandHandlerBase<VmHighAvailabilityCommand>(serviceProvider)
{
    protected override async Task<PveTaskResult> ExecuteAsync(VmHighAvailabilityCommand command, CancellationToken cancellationToken)
    {
        var vm = await GetVmAsync(command.ClusterName, command.VmId);
        var client = await GetPveClientAsync(command.ClusterName);
        var sid = $"{(vm.VmType == VmType.Qemu ? "vm" : "ct")}:{vm.VmId}";
        var group = string.IsNullOrWhiteSpace(command.Group) ? null : command.Group;

        if (command.Action == VmHighAvailabilityAction.Add
            && group != null
            && await PveAdminHelper.GetNodeMajorVersionAsync(client, vm.Node) >= 9)
        {
            throw new InvalidOperationException("HA groups are replaced by HA rules in Proxmox VE 9: leave the group empty");
        }

        var result = command.Action switch
        {
            VmHighAvailabilityAction.Add => await client.Cluster.Ha.Resources.Create(sid: sid,
                                                                                     group: group,
                                                                                     max_relocate: command.MaxRelocate,
                                                                                     max_restart: command.MaxRestart,
                                                                                     state: command.State),
            VmHighAvailabilityAction.Remove => await client.Cluster.Ha.Resources[sid].Delete(),
            VmHighAvailabilityAction.Migrate => await client.Cluster.Ha.Resources[sid].Migrate.Migrate(GetTargetNode(command)),
            VmHighAvailabilityAction.Relocate => await client.Cluster.Ha.Resources[sid].Relocate.Relocate(GetTargetNode(command)),
            _ => throw new InvalidEnumArgumentException(),
        };
        return PveTaskResult.Success(result, command.ClusterName);
    }

    private static string GetTargetNode(VmHighAvailabilityCommand command)
        => string.IsNullOrWhiteSpace(command.TargetNode)
            ? throw new InvalidOperationException($"Target node is required for {command.Action}")
            : command.TargetNode;
}
