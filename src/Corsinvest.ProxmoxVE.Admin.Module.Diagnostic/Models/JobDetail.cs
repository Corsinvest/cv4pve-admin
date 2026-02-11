using Corsinvest.ProxmoxVE.Diagnostic.Api;

namespace Corsinvest.ProxmoxVE.Admin.Module.Diagnostic.Models;

public class JobDetail : IId, IDescription
{
    public int Id { get; set; }
    [Required] public JobResult JobResult { get; set; } = default!;
    public string IdResource { get; set; } = default!;
    public DiagnosticResultContext Context { get; set; }
    public string Description { get; set; } = default!;
    public DiagnosticResultGravity Gravity { get; set; }
    public bool IsIgnoredIssue { get; set; }
    public string SubContext { get; internal set; } = default!;
}
