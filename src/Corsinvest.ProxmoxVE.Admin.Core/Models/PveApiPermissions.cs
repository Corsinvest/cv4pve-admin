/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Models;

public static class PveApiPermissions
{
    // ===== Global / User =====
    public const string GroupAllocate = "Group.Allocate";
    public const string PermissionsModify = "Permissions.Modify";
    public const string UserModify = "User.Modify";

    // ===== Mapping (LDAP / AD / SSO) =====
    public static class Mapping
    {
        public const string Audit = "Mapping.Audit";
        public const string Modify = "Mapping.Modify";
        public const string Use = "Mapping.Use";
    }

    // ===== Realm =====
    public static class Realm
    {
        public const string Allocate = "Realm.Allocate";
        public const string AllocateUser = "Realm.AllocateUser";
    }

    // ===== System / Node =====
    public static class Sys
    {
        public const string Audit = "Sys.Audit";
        public const string Console = "Sys.Console";
        public const string Incoming = "Sys.Incoming";
        public const string Modify = "Sys.Modify";
        public const string PowerMgmt = "Sys.PowerMgmt";
        public const string Syslog = "Sys.Syslog";
        public const string AccessNetwork = "Sys.AccessNetwork";
    }

    // ===== Virtual Machines / Containers =====
    public static class Vm
    {
        public const string Allocate = "VM.Allocate";
        public const string Audit = "VM.Audit";
        public const string Backup = "VM.Backup";
        public const string Clone = "VM.Clone";
        public const string Console = "VM.Console";
        public const string Migrate = "VM.Migrate";
        public const string Monitor = "VM.Monitor";
        public const string PowerMgmt = "VM.PowerMgmt";
        public const string Replicate = "VM.Replicate";
        public const string Snapshot = "VM.Snapshot";
        public const string SnapshotRollback = "VM.Snapshot.Rollback";

        // ---- Config ----
        public static class Config
        {
            public const string Cdrom = "VM.Config.CDROM";
            public const string Cloudinit = "VM.Config.Cloudinit";
            public const string Cpu = "VM.Config.CPU";
            public const string Disk = "VM.Config.Disk";
            public const string HwType = "VM.Config.HWType";
            public const string Memory = "VM.Config.Memory";
            public const string Network = "VM.Config.Network";
            public const string Options = "VM.Config.Options";
        }

        // ---- Guest Agent ----
        public static class GuestAgent
        {
            public const string Audit = "VM.GuestAgent.Audit";
            public const string FileRead = "VM.GuestAgent.FileRead";
            public const string FileSystemMgmt = "VM.GuestAgent.FileSystemMgmt";
            public const string FileWrite = "VM.GuestAgent.FileWrite";
            public const string Unrestricted = "VM.GuestAgent.Unrestricted";
        }
    }

    // ===== Storage / Datastore =====
    public static class Datastore
    {
        public const string Allocate = "Datastore.Allocate";
        public const string AllocateSpace = "Datastore.AllocateSpace";
        public const string AllocateTemplate = "Datastore.AllocateTemplate";
        public const string Audit = "Datastore.Audit";
    }

    // ===== Pools =====
    public static class Pool
    {
        public const string Allocate = "Pool.Allocate";
        public const string Audit = "Pool.Audit";
    }

    // ===== SDN =====
    public static class Sdn
    {
        public const string Allocate = "SDN.Allocate";
        public const string Audit = "SDN.Audit";
        public const string Use = "SDN.Use";
    }

    // ===== HA (High Availability) =====
    public static class Ha
    {
        public const string Audit = "HA.Audit";
        public const string Modify = "HA.Modify";
    }

    // ===== Cluster =====
    public static class Cluster
    {
        public const string Audit = "Cluster.Audit";
        public const string Modify = "Cluster.Modify";
    }

    // ===== Backup =====
    public static class Backup
    {
        public const string Audit = "Backup.Audit";
        public const string Modify = "Backup.Modify";
    }

    // ===== Firewall =====
    public static class Firewall
    {
        public const string Audit = "Firewall.Audit";
        public const string Modify = "Firewall.Modify";
    }

    // ===== SDN Zones / Vnets =====
    public static class Network
    {
        public const string Audit = "Network.Audit";
        public const string Modify = "Network.Modify";
    }
}
