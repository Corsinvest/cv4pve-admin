using Prometheus;

namespace Corsinvest.ProxmoxVE.Admin.Module.MetricsExporter;

internal class Info
{
    public DateTime? LastRequest { get; set; }
    public long CountRequest { get; set; }
    public CollectorRegistry Registry { get; set; } = default!;
}
