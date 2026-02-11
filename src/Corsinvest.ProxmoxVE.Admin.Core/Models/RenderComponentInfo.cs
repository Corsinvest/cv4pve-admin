namespace Corsinvest.ProxmoxVE.Admin.Core.Models;

public record RenderComponentInfo(Type Type, IDictionary<string, object>? Parameters = null);
