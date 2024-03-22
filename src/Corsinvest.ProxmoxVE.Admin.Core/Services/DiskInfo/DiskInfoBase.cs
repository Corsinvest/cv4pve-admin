/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Services.DiskInfo;

public abstract class DiskInfoBase(long vmId, string disk, string host, string spaceName)
{
    public long VmId { get; } = vmId;
    public string Disk { get; } = disk;
    public string Host { get; } = host;
    public string SpaceName { get; } = spaceName;
    public abstract string Type { get; }

    public List<DiskInfoSnapshot> Snapshots { get; } = [];

    public class DiskInfoSnapshot(string name, double size, bool replication)
    {
        public string Name { get; } = name;
        public double Size { get; } = size;
        public bool Replication { get; } = replication;
    }
}
