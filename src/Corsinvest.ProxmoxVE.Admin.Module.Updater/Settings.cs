using Corsinvest.ProxmoxVE.Admin.Core.Notifier;
using Corsinvest.ProxmoxVE.Admin.Module.Updater.Helpers;

namespace Corsinvest.ProxmoxVE.Admin.Module.Updater;

public class Settings : JobScheduleBase, IModuleSettings, INotifierConfigurationsSettings
{
    public Settings() => CronExpression = "0 */12 * * *";

    [Required] public string ClusterName { get; set; } = default!;
    [Required] public string ScriptWindowsSearchUpdate { get; set; } = ActionHelper.GetDefaultScript(ScriptType.WindowsSearchUpdate);
    //[Required] public string ScriptWindowsExecuteUpdate { get; set; } = ActionHelper.GetDefaultScript(ScriptType.WindowsExecuteUpdate);
    [Required] public string ScriptLinuxSearchUpdate { get; set; } = ActionHelper.GetDefaultScript(ScriptType.LinuxSearchUpdate);
    //[Required] public string ScriptLinuxExecuteUpdate { get; set; } = ActionHelper.GetDefaultScript(ScriptType.LinuxExecuteUpdate);
    public IEnumerable<string> NotifierConfigurations { get; set; } = [];
}
