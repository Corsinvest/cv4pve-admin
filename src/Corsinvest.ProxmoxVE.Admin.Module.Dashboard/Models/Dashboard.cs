using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Corsinvest.ProxmoxVE.Admin.Module.Dashboard.Models;

public class Dashboard : IId, IName
{
    [JsonIgnore] public int Id { get; set; }
    [Required] public string Name { get; set; } = default!;
    [Required] public string UserId { get; set; } = default!;
    public bool AllowClustersSelect { get; set; }
    public int RefreshInterval { get; set; }

    public ICollection<Widget> Widgets { get; set; } = [];

    public static IEnumerable<Dashboard> GetDefaults()
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Corsinvest.ProxmoxVE.Admin.Module.Dashboard.dashboards.json")!;
        using var reader = new StreamReader(stream);
        var json = reader.ReadToEnd();
        return JsonSerializer.Deserialize<IEnumerable<Dashboard>>(json)!;
    }
}
