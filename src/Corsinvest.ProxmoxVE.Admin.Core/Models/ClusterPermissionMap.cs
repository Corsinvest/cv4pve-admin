/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth;

namespace Corsinvest.ProxmoxVE.Admin.Core.Models;

public static class ClusterPermissionMap
{
    public static readonly IReadOnlyDictionary<string, string> All = new Dictionary<string, string>
    {
        // VM
        [ClusterPermissions.Vm.Audit.Key] = PveApiPermissions.Vm.Audit,
        [ClusterPermissions.Vm.Console.Key] = PveApiPermissions.Vm.Console,
        [ClusterPermissions.Vm.PowerManagement.Key] = PveApiPermissions.Vm.PowerMgmt,
        [ClusterPermissions.Vm.Snapshot.Key] = PveApiPermissions.Vm.Snapshot,
        [ClusterPermissions.Vm.SnapshotRallback.Key] = PveApiPermissions.Vm.SnapshotRollback,
        [ClusterPermissions.Vm.Replication.Key] = PveApiPermissions.Vm.Replicate,
        [ClusterPermissions.Vm.ReplicationScheduleNow.Key] = PveApiPermissions.Vm.Replicate,
        [ClusterPermissions.Vm.Backup.Key] = PveApiPermissions.Vm.Backup,
        [ClusterPermissions.Vm.BackupRestore.Key] = PveApiPermissions.Datastore.AllocateSpace,
        [ClusterPermissions.Vm.BackupRestoreFile.Key] = PveApiPermissions.Datastore.AllocateSpace,

        // Node
        [ClusterPermissions.Node.Audit.Key] = PveApiPermissions.Sys.Audit,
        [ClusterPermissions.Node.Console.Key] = PveApiPermissions.Sys.Console,
        [ClusterPermissions.Node.PowerManagement.Key] = PveApiPermissions.Sys.PowerMgmt,
        [ClusterPermissions.Node.Replication.Key] = PveApiPermissions.Vm.Replicate,
        [ClusterPermissions.Node.ReplicationScheduleNow.Key] = PveApiPermissions.Vm.Replicate,
    };
}
