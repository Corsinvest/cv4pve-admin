/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Module.AIServer;

public static class Permissions
{
    public static string BaseName { get; } = nameof(AIServer);

    public static class Tools
    {
        private static string BaseName { get; } = new[] { Permissions.BaseName, nameof(Tools) }.JoinAsString(".");

        // VM tools
        public static Permission ListVms { get; } = new(BaseName, nameof(AIServer.Tools.VmTools.ListVms), "List VMs");
        public static Permission ListSnapshots { get; } = new(BaseName, nameof(AIServer.Tools.VmTools.ListSnapshots), "List VM snapshots");
        public static Permission GetVmConfig { get; } = new(BaseName, nameof(AIServer.Tools.VmTools.GetVmConfig), "Get VM configuration");
        public static Permission ChangeVmState { get; } = new(BaseName, nameof(AIServer.Tools.VmTools.ChangeVmState), "Change VM power state (start/stop/shutdown/reset)");
        public static Permission CreateVmSnapshot { get; } = new(BaseName, nameof(AIServer.Tools.VmTools.CreateVmSnapshot), "Create VM snapshot");
        public static Permission DeleteVmSnapshot { get; } = new(BaseName, nameof(AIServer.Tools.VmTools.DeleteVmSnapshot), "Delete VM snapshot");
        public static Permission RollbackVmSnapshot { get; } = new(BaseName, nameof(AIServer.Tools.VmTools.RollbackVmSnapshot), "Rollback VM to snapshot");
        public static Permission MigrateVm { get; } = new(BaseName, nameof(AIServer.Tools.VmTools.MigrateVm), "Migrate VM to another node");
        public static Permission BackupVm { get; } = new(BaseName, nameof(AIServer.Tools.VmTools.BackupVm), "Create VM backup");
        public static Permission ListVmRrdData { get; } = new(BaseName, nameof(AIServer.Tools.VmTools.ListVmRrdData), "List VM RRD data");

        // Node tools
        public static Permission ListNodes { get; } = new(BaseName, nameof(AIServer.Tools.NodeTools.ListNodes), "List cluster nodes");
        public static Permission GetNodeStatus { get; } = new(BaseName, nameof(AIServer.Tools.NodeTools.GetNodeStatus), "Get node status");
        public static Permission ListReplications { get; } = new(BaseName, nameof(AIServer.Tools.NodeTools.ListReplications), "List cluster replications");
        public static Permission ListNodeRrdData { get; } = new(BaseName, nameof(AIServer.Tools.NodeTools.ListNodeRrdData), "List node RRD data");
        public static Permission ListTasks { get; } = new(BaseName, nameof(AIServer.Tools.NodeTools.ListTasks), "List cluster tasks");

        // Storage tools
        public static Permission ListStorage { get; } = new(BaseName, nameof(AIServer.Tools.StorageTools.ListStorage), "List cluster storage");
        public static Permission ListPools { get; } = new(BaseName, nameof(AIServer.Tools.StorageTools.ListPools), "List cluster pools");
        public static Permission ListBackups { get; } = new(BaseName, nameof(AIServer.Tools.StorageTools.ListBackups), "List backups available on storage");
        public static Permission ListStorageContent { get; } = new(BaseName, nameof(AIServer.Tools.StorageTools.ListStorageContent), "List storage content (ISO, templates, images)");
        public static Permission ListBackupJobs { get; } = new(BaseName, nameof(AIServer.Tools.StorageTools.ListBackupJobs), "List cluster backup jobs");

        // Cluster tools
        public static Permission GetClusterStatus { get; } = new(BaseName, nameof(AIServer.Tools.ClusterTools.GetClusterStatus), "Get cluster health summary");
    }
}
