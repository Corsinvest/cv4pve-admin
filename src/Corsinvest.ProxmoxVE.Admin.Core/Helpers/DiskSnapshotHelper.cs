/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Helpers;

public static class DiskSnapshotHelper
{
    public static double CalculateSnapshots(long vmid, string snapName, IEnumerable<DiskSnapshotInfo> disks)
        => disks.Where(a => a.VmId == vmid, vmid > 0)
                .SelectMany(a => a.Snapshots)
                .Where(a => a.Name == snapName)
                .Select(a => a.Size)
                .DefaultIfEmpty(0)
                .Sum();

    public static double CalculateSnapshots(string node, long vmid, IEnumerable<DiskSnapshotInfo> disks, bool replication)
        => disks.Where(a => a.VmId == vmid && (!a.HostContainSnapshot || a.Host == node))
                .SelectMany(a => a.Snapshots)
                .Where(a => a.Replication, replication)
                .Select(a => a.Size)
                .DefaultIfEmpty(0)
                .Sum();

    public static double CalculateSnapshots(string node, IEnumerable<DiskSnapshotInfo> disks, bool replication)
        => disks.Where(a => a.Host == node)
                .SelectMany(a => a.Snapshots)
                .Where(a => a.Replication, replication)
                .Select(a => a.Size)
                .DefaultIfEmpty(0)
                .Sum();

    public static double CalculateSnapshots(string node, string storage, IEnumerable<DiskSnapshotInfo> disks, bool replication)
        => disks.Where(a => a.Host == node && a.SpaceName == storage
                            && (!a.HostContainSnapshot || a.Host == node))
                .SelectMany(a => a.Snapshots)
                .Where(a => a.Replication, replication)
                .Select(a => a.Size)
                .DefaultIfEmpty(0)
                .Sum();

    public static double CalculateSnapshots(string node, long vmId, string storage, string fileName, IEnumerable<DiskSnapshotInfo> disks, bool replication)
        => disks.Where(a => a.Host == node
                            && a.SpaceName == storage
                            && a.VmId == vmId
                            && a.Disk == fileName)
                .SelectMany(a => a.Snapshots)
                .Where(a => a.Replication, replication)
                .Select(a => a.Size)
                .DefaultIfEmpty(0)
                .Sum();

    public static double CalculateSnapshot(string node, long vmId, string snapName, IEnumerable<DiskSnapshotInfo> disks)
        => disks.Where(a => a.VmId == vmId && a.Host == node)
                .SelectMany(a => a.Snapshots)
                .Where(a => !a.Replication && a.Name == snapName)
                .Sum(a => a.Size);
}
