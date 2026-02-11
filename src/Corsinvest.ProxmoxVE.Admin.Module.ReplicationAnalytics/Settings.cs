using Corsinvest.ProxmoxVE.Admin.Core.Modularity;

namespace Corsinvest.ProxmoxVE.Admin.Module.ReplicationAnalytics;

public class Settings : JobScheduleBase, IModuleSettings
{
    public Settings() => CronExpression = "*/5 * * * *";
    [Required] public string ClusterName { get; set; } = default!;
    public int MaxDaysLogs { get; set; } = 30;
}
