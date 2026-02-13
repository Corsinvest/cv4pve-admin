/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Helpers;

public static class DiskInfoHelper
{
    public static double CalculateSnapshots(long vmid, string snapName, IEnumerable<VmDiskInfo> disks)
        => disks.Where(a => a.VmId == vmid, vmid > 0)
                .SelectMany(a => a.Snapshots)
                .Where(a => a.Name == snapName)
                .Select(a => a.Size)
                .DefaultIfEmpty(0)
                .Sum();

    public static double CalculateSnapshots(string node, long vmid, IEnumerable<VmDiskInfo> disks, bool replication)
        => disks.Where(a => a.VmId == vmid && (!a.HostContainSnapshot || a.Host == node))
                .SelectMany(a => a.Snapshots)
                .Where(a => a.Replication, replication)
                .Select(a => a.Size)
                .DefaultIfEmpty(0)
                .Sum();

    public static double CalculateSnapshots(string node, IEnumerable<VmDiskInfo> disks, bool replication)
        => disks.Where(a => a.Host == node)
                .SelectMany(a => a.Snapshots)
                .Where(a => a.Replication, replication)
                .Select(a => a.Size)
                .DefaultIfEmpty(0)
                .Sum();

    public static double CalculateSnapshots(string node, string storage, IEnumerable<VmDiskInfo> disks, bool replication)
        => disks.Where(a => a.Host == node && a.SpaceName == storage
                            && (!a.HostContainSnapshot || a.Host == node))
                .SelectMany(a => a.Snapshots)
                .Where(a => a.Replication, replication)
                .Select(a => a.Size)
                .DefaultIfEmpty(0)
                .Sum();

    public static double CalculateSnapshots(string node, long vmId, string storage, string fileName, IEnumerable<VmDiskInfo> disks, bool replication)
        => disks.Where(a => a.Host == node
                            && a.SpaceName == storage
                            && a.VmId == vmId
                            && a.Disk == fileName)
                .SelectMany(a => a.Snapshots)
                .Where(a => a.Replication, replication)
                .Select(a => a.Size)
                .DefaultIfEmpty(0)
                .Sum();
}
