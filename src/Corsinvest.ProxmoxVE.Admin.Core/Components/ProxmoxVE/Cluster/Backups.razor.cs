using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components.ProxmoxVE.Cluster;

public partial class Backups(IAdminService adminService) : IClusterName
{
    [EditorRequired, Parameter] public string ClusterName { get; set; } = default!;
    [Parameter] public string Style { get; set; } = default!;

    private IEnumerable<ClusterBackup> Items { get; set; } = default!;
    protected override async Task OnInitializedAsync()
    {
        var client = await adminService[ClusterName].GetPveClientAsync();
        Items = (await client.Cluster.Backup.GetAsync()).OrderBy(a => a.Id);
    }
}
