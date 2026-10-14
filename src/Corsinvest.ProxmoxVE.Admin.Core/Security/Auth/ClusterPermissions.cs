/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth.Permissions;

namespace Corsinvest.ProxmoxVE.Admin.Core.Security.Auth;

public static class ClusterPermissions
{
    public static string BaseName { get; } = "Cluster";

    public static Role RoleVmUser { get; } = new($"{BaseName}.VMUser",
                                                 "Cluster VM User",
                                                 false,
                                                 true,
                                                 Vm.Data.Permissions
                                                        .CombineWith(Vm.Audit)
                                                        .CombineWith(Vm.Console)
                                                        .CombineWith(Vm.PowerManagement)

                                                        //Replication
                                                        .CombineWith(Vm.Replication)
                                                        .CombineWith(Vm.ReplicationScheduleNow)

                                                        //Snapshot
                                                        .CombineWith(Vm.Snapshot)
                                                        .CombineWith(Vm.SnapshotRallback)

                                                        //Backup
                                                        .CombineWith(Vm.Backup)
                                                        .CombineWith(Vm.BackupRestore)
                                                        .CombineWith(Vm.BackupRestoreFile)

                                                        .CombineWith(Vm.Migrate));

    public static Role RoleStorageUser { get; } = new($"{BaseName}.StorageUser",
                                                      "Cluster Storage User",
                                                      false,
                                                      true,
                                                      Storage.Data.Permissions.CombineWith(Storage.DeleteContent));
    public static Role RoleNodeUser { get; } = new($"{BaseName}.NodeUser",
                                                   "Cluster Node User",
                                                   false,
                                                   true,
                                                   Node.Data.Permissions
                                                            .CombineWith(Node.Audit)
                                                            .CombineWith(Node.Console)
                                                            .CombineWith(Node.PowerManagement)

                                                            //Replication
                                                            .CombineWith(Node.Replication)
                                                            .CombineWith(Node.ReplicationScheduleNow));

    public static Role RoleAdmin { get; } = new($"{BaseName}.Admin",
                                                "Cluster Admin User",
                                                false,
                                                true,
                                                RoleNodeUser.Permissions
                                                            .CombineWith(RoleStorageUser)
                                                            .CombineWith(RoleVmUser)
                                                            .CombineWith(Cluster.SelectCluster)
                                                            .CombineWith(Vm.Clone)
                                                            .CombineWith(Vm.Resize)
                                                            .CombineWith(Vm.Template)
                                                            .CombineWith(Vm.HighAvailability));

    public static class Cluster
    {
        public static Permission SelectCluster { get; } = new(BaseName, "Select", "Select cluster");
    }

    public static class Node
    {
        //public static PermissionsRead Data { get; } = new($"{typeof(ClusterPermissions).FullName}.{nameof(Node)}.{nameof(Data)}");
        public static PermissionsRead Data { get; } = new(BaseName, nameof(Node));

        public static Permission Console { get; } = new(Data.Prefix, nameof(Console), "Console");
        public static Permission PowerManagement { get; } = new(Data.Prefix, nameof(PowerManagement), "Power Management");
        public static Permission Audit { get; } = new(Data.Prefix, nameof(Audit), "Audit");
        public static Permission Replication { get; } = new(Data.Prefix, nameof(Replication), "Replication manager");
        public static Permission ReplicationScheduleNow { get; } = new(Replication.Key, "ScheduleNow", "Replication Schedule Now");
    }

    public static class Storage
    {
        //public static PermissionsRead Data { get; } = new($"{typeof(ClusterPermissions).FullName}.{nameof(Storage)}.{nameof(Data)}");
        public static PermissionsRead Data { get; } = new(BaseName, nameof(Storage));
        public static Permission DeleteContent { get; } = new(Data.Prefix, nameof(DeleteContent), "Delete Content");
    }

    public static class Vm
    {
        public static PermissionsRead Data { get; } = new(BaseName, nameof(Vm));

        public static Permission Snapshot { get; } = new(Data.Prefix, nameof(Snapshot), "Snapshot manager");
        public static Permission SnapshotRallback { get; } = new(Snapshot.Key, "Rallback", "Snapshot Rollback");

        public static Permission Backup { get; } = new(Data.Prefix, nameof(Backup), "Backup manager");
        public static Permission BackupRestore { get; } = new(Backup.Key, "Restore", "Backup Restore");
        public static Permission BackupRestoreFile { get; } = new(Backup.Key, "RestoreFile", "Backup Restore File");

        public static Permission Console { get; } = new(Data.Prefix, nameof(Console), "Console");
        public static Permission PowerManagement { get; } = new(Data.Prefix, nameof(PowerManagement), "Power Management");

        public static Permission Audit { get; } = new(Data.Prefix, nameof(Audit), "Audit");

        public static Permission Replication { get; } = new(Data.Prefix, nameof(Replication), "Replication manager");
        public static Permission ReplicationScheduleNow { get; } = new(Replication.Key, "ScheduleNow", "Replication Schedule Now");

        public static Permission Migrate { get; } = new(Data.Prefix, nameof(Migrate), "Migrate");
        public static Permission Clone { get; } = new(Data.Prefix, nameof(Clone), "Clone");
        public static Permission Resize { get; } = new(Data.Prefix, nameof(Resize), "Resize disk");
        public static Permission Template { get; } = new(Data.Prefix, nameof(Template), "Convert to template");
        public static Permission HighAvailability { get; } = new(Data.Prefix, nameof(HighAvailability), "High Availability");
    }
}
