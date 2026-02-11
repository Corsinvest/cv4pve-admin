using Corsinvest.ProxmoxVE.Admin.Core.Modularity;

namespace Corsinvest.ProxmoxVE.Admin.Module.BackupAnalytics;

public class Settings : JobScheduleBase, IModuleSettings
{
    public Settings() => CronExpression = "0 */1 * * *";
    [Required] public string ClusterName { get; set; } = default!;

    [Display(Name = "Max days logs")]
    public int MaxDaysLogs { get; set; } = 30;
}
