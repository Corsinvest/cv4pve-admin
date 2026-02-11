namespace Corsinvest.ProxmoxVE.Admin.Module.Updater.Models;

internal interface IUpdateInfo
{
    UpdateInfoStatus UpdateScanStatus { get; set; }
    bool UpdateNormalAvailable { get; set; }
    bool UpdateSecurityAvailable { get; set; }
    bool UpdateRequireReboot { get; set; }
}
