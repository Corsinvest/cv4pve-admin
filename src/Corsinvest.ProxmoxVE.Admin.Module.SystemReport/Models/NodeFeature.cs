namespace Corsinvest.ProxmoxVE.Admin.Module.SystemReport.Models;

[Flags]
public enum NodeFeature
{
    All = 0,
    Services = 1,
    Network = 2,
    Replications = 4,
    Disks = 8,
    SmartDisks = 16,
    RrdData = 32,
    AptInfo = 64,
    PackagesVersions = 128,
    SnapshotsSize = 256
}
