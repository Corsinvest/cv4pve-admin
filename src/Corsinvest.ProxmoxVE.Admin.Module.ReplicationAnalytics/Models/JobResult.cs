namespace Corsinvest.ProxmoxVE.Admin.Module.ReplicationAnalytics.Models;

public class JobResult : IClusterName, IId
{
    public int Id { get; set; }
    [Required] public string ClusterName { get; set; } = default!;

    public string JobId { get; set; } = default!;
    public DateTime Start { get; set; }
    public DateTime? End { get; set; }

    public TimeSpan Duration
        => End.HasValue
            ? (End - Start).Value
            : TimeSpan.Zero;

    //public double DurationCalc => End.HasValue ? (End - Start).Value.TotalSeconds : 0;
    //public double Duration { get; set; }

    //[Display(Name = "Duration")]
    //public string DurationText
    //    => End.HasValue
    //        ? (End - Start).Value.ToString("hh':'mm':'ss")
    //        : "00:00:00";

    public string VmId { get; set; } = default!;
    public double Size { get; set; }

    //[Display(Name = "Size")]
    //public string SizeString => FormatHelper.FromBytes(Size);

    public string Logs { get; set; } = default!;
    public DateTime LastSync { get; set; }
    public string? Error { get; set; }
    public bool Status { get; set; }
    public string Source { get; set; } = default!;
    public string Target { get; set; } = default!;
}
