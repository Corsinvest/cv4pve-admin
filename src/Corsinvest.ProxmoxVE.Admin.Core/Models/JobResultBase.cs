namespace Corsinvest.ProxmoxVE.Admin.Core.Models;

public class JobResultBase
{
    public DateTime Start { get; set; }
    public DateTime? End { get; set; }

    public TimeSpan Duration
        => End.HasValue
            ? (End - Start).Value
            : TimeSpan.Zero;

    public bool Status { get; set; }
    public string Logs { get; set; } = default!;
}
