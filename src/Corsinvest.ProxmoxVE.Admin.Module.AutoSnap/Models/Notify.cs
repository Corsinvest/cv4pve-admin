namespace Corsinvest.ProxmoxVE.Admin.Module.AutoSnap.Models;

public enum Notify
{
    None = 0,

    Allways = 1,

    [Display(Description = "On Failure Only")]
    OnFailureOnly = 2
}
