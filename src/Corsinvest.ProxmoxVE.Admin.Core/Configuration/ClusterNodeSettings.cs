namespace Corsinvest.ProxmoxVE.Admin.Core.Configuration;

public class ClusterNodeSettings
{
    [Required]
    public string IPAddress { get; set; } = default!;

    public int ApiPort { get; set; } = 8006;
}
