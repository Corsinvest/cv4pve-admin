
namespace Corsinvest.ProxmoxVE.Admin.Module.System.Components;

public class SettingSection
{
    public string Title { get; set; } = default!;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = "settings";
    public Type ComponentType { get; set; } = default!;
}
