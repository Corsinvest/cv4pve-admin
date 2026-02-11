namespace Corsinvest.ProxmoxVE.Admin.Module.AutoSnap.Models;

public class JobResult : JobResultBase, IId
{
    public int Id { get; set; }
    [Required] public JobSchedule Job { get; set; } = default!;
    public string SnapName { get; set; } = default!;
}
