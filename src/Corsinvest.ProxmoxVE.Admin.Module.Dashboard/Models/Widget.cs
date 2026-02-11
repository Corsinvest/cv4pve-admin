using System.Text.Json.Serialization;

namespace Corsinvest.ProxmoxVE.Admin.Module.Dashboard.Models;

public class Widget : IId
{
    [JsonIgnore] public int Id { get; set; }
    [Required] public string Title { get; set; } = default!;
    [JsonIgnore, Required] public Dashboard Dashboard { get; set; } = default!;
    public string? TitleCss { get; set; }
    public string? BodyCss { get; set; }

    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string? SettingsJson { get; set; }
    [Required] public string ModuleType { get; set; } = default!;
    [Required] public string ModuleWidgetType { get; set; } = default!;

    public ModuleWidget? GetModuleWidget(IModuleService moduleService)
    {
        var module = moduleService.Get(ModuleType);
        return module?.Widgets.FirstOrDefault(a => a.RenderInfo.Type.FullName == ModuleWidgetType);
    }
}
