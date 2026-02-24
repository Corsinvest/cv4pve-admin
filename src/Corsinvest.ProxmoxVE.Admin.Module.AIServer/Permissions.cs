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

        public static Permission ListNodes { get; } = new(BaseName, nameof(AIServer.Tools.VmTools.ListNodes), "List cluster nodes ");
        public static Permission ListVms { get; } = new(BaseName, nameof(AIServer.Tools.VmTools.ListVms), "List VMs");
        public static Permission ListSnapshots { get; } = new(BaseName, nameof(AIServer.Tools.VmTools.ListSnapshots), "List VM snapshots");
        public static Permission ListPools { get; } = new(BaseName, nameof(AIServer.Tools.VmTools.ListPools), "List cluster pools");
        public static Permission ListStorage { get; } = new(BaseName, nameof(AIServer.Tools.VmTools.ListStorage), "List cluster storage");
    }
}
