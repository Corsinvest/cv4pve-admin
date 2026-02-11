namespace Corsinvest.ProxmoxVE.Admin.Module.SystemReport.Models;

[Flags]
public enum VmFeature
{
    All = 0,
    Network = 1,
    Disks = 2,
    QemuGuestInfo = 4,
    RrdData = 8,
    Backup = 16,
    Replications = 32,
    Snapshots = 64,
    SnapshotsSize = 128
}
