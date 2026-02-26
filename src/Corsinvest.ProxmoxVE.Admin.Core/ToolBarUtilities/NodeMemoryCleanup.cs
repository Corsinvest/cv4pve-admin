/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;

namespace Corsinvest.ProxmoxVE.Admin.Core.ToolBarUtilities;

public class NodeMemoryCleanup(IAdminService adminService,
                               IStringLocalizer<NodeMemoryCleanup> L,
                               NotificationService notificationService) : IToolBarUtility<IClusterResourceNode>
{
    public string Icon { get; } = "cleaning_services";
    public string Text { get; } = "Memory Cleanup";
    public ToolBarUtilityType Type { get; } = ToolBarUtilityType.Node;
    public bool RequireConfirm { get; } = true;
    public bool IsVIsible(IClusterResourceNode item) => true;

    public async Task ExecuteAsync(string clusterName, IClusterResourceNode item)
    {
        var command = "sync && echo 3 > /proc/sys/vm/drop_caches && echo 1 > /proc/sys/vm/compact_memory";
        var result = await adminService[clusterName].SshExecuteAsync(item.Node,true, [command]);
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
    }
}
