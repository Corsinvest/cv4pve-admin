/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Models;

public record VmDiskInfo(long VmId,
                         string Disk,
                         string Host,
                         string SpaceName,
                         bool HostContainSnapshot,
                         string Type,
                         ICollection<VmDiskInfo.Snapshot> Snapshots,
                         Dictionary<string, object> Extra)
{
    public record Snapshot(string Name, double Size, bool Replication, DateTime Date);
}
