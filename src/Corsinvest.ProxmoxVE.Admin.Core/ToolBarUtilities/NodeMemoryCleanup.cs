/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth;
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth.Permissions;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;

namespace Corsinvest.ProxmoxVE.Admin.Core.ToolBarUtilities;

public class NodeMemoryCleanup(IAdminService adminService,
                               IPermissionService permissionService,
                               IAuditService auditService,
                               IStringLocalizer<NodeMemoryCleanup> L,
                               NotificationService notificationService) : ToolBarUtility<IClusterResourceNode>(auditService)
{
    public override string Icon { get; } = "cleaning_services";
    public override string Text { get; } = "Memory Cleanup";

    protected override string AuditAction { get; } = "Node.MemoryCleanup";

    protected override string GetAuditContext(string clusterName, IClusterResourceNode item)
        => $"Cluster: {clusterName} Node: {item.Node}";

    public override Task<bool> HasPermissionAsync(string clusterName, IClusterResourceNode item)
        => permissionService.HasNodeAsync(clusterName, ClusterPermissions.Node.PowerManagement, item.Node);

    protected override async Task<bool> ExecuteCoreAsync(string clusterName, IClusterResourceNode item)
    {
        var command = "sync && echo 3 > /proc/sys/vm/drop_caches && echo 1 > /proc/sys/vm/compact_memory";
        var result = await adminService[clusterName].SshExecuteAsync(item.Node, true, [command]);
        if (result[0].IsSuccess)
        {
            notificationService.Info(L["Node memory freed successfully!"]);
        }
        else if (result[0].IsSshNotConfigured)
        {
            notificationService.Warning(result[0].StdErr);
        }
        else
        {
            notificationService.Error(result[0].StdErr);
        }

        return result[0].IsSuccess;
    }
}
