using Corsinvest.ProxmoxVE.Admin.Core.Modularity;

namespace Corsinvest.ProxmoxVE.Admin.Module.SystemReport;

public class Settings : IModuleSettings
{
    [Required] public string ClusterName { get; set; } = default!;
    public bool Enabled { get; set; }
}
