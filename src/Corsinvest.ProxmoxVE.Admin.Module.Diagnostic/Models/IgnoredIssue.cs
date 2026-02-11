using Corsinvest.ProxmoxVE.Diagnostic.Api;

namespace Corsinvest.ProxmoxVE.Admin.Module.Diagnostic.Models;

public class IgnoredIssue : IClusterName, IId
{
    public int Id { get; set; }
    [Required] public string ClusterName { get; set; } = default!;
    public string? IdResource { get; set; }
    public DiagnosticResultGravity Gravity { get; set; }
    public DiagnosticResultContext Context { get; set; }
    public string? SubContext { get; set; }
    public string? Description { get; set; }
}
