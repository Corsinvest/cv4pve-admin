namespace Corsinvest.ProxmoxVE.Admin.Module.NodeProtect.Models;

public record Provider(string Name,
                       RenderComponentInfo Render,
                       RenderComponentInfo Settings,
                       string Icon);
