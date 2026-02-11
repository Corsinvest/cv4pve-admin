namespace Corsinvest.ProxmoxVE.Admin.Module.SystemReport.Models;

[Flags]
public enum StorageFeature
{
    All = 0,
    Content = 1,
    RrdData = 2
}
