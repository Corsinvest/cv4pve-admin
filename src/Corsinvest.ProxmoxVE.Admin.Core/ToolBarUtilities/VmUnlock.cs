/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth;
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth.Permissions;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;

namespace Corsinvest.ProxmoxVE.Admin.Core.ToolBarUtilities;

public class VmUnlock(IAdminService adminService,
                      IPermissionService permissionService,
                      IAuditService auditService) : ToolBarUtility<IClusterResourceVm>(auditService)
{
    public override string Icon { get; } = "lock_open";
    public override string Text { get; } = "Unlock";
    public override bool IsVIsible(IClusterResourceVm item) => item.IsLocked;

    protected override string AuditAction { get; } = "Vm.Unlock";

    protected override string GetAuditContext(string clusterName, IClusterResourceVm item)
        => $"Cluster: {clusterName} VM: {item.VmId}";

    public override Task<bool> HasPermissionAsync(string clusterName, IClusterResourceVm item)
        => permissionService.HasVmAsync(clusterName, ClusterPermissions.Vm.PowerManagement, item.VmId);

    protected override async Task<bool> ExecuteCoreAsync(string clusterName, IClusterResourceVm item)
    {
        var client = await adminService[clusterName].GetPveClientAsync();
        await client.VmUnlockAsync(item.Node, item.VmType, item.VmId);
        return true;
    }
}
