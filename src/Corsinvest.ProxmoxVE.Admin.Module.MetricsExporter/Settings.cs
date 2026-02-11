using System.ComponentModel.DataAnnotations;
using Corsinvest.ProxmoxVE.Admin.Core.Helpers;
using Corsinvest.ProxmoxVE.Admin.Core.Models;
using Corsinvest.ProxmoxVE.Admin.Core.Modularity;

namespace Corsinvest.ProxmoxVE.Admin.Module.MetricsExporter;

public class Settings : IModuleSettings
{
    [Required] public string ClusterName { get; set; } = ApplicationHelper.AllClusterName;
    public bool Enabled { get; set; }

    public PrometheusSettings Prometheus { get; set; } = new();

    public class PrometheusSettings : IEnabled
    {
        public bool Enabled { get; set; }

        [Required]
        public string ExporterPrefix { get; set; } = "cv4pve";

        [Required]
        [Encrypt]
        public string Token { get; set; } = string.Empty;
    }
}
