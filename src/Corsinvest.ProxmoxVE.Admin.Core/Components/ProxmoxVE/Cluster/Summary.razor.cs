using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components.ProxmoxVE.Cluster;

public partial class Summary(IAdminService adminService) : IClusterName
{
    [EditorRequired, Parameter] public string ClusterName { get; set; } = default!;

    private bool CephInstalled { get; set; }

    //private List<ItemStatus<VmType>> VmsStatus { get; set; } = [];
    private ICollection<ResourceUsage> DataUsages { get; set; } = [];
    //private List<ItemStatus> NodeHealts { get; set; } = [];
    private ItemStatus StatusInfo { get; set; } = new();
    private IEnumerable<ClusterStatus> Nodes { get; set; } = [];
    private ItemStatus<object> CephStatus { get; set; } = default!;

    private class ClusterStatus
    {
        public string Name { get; set; } = default!;
        public int? Id { get; set; }
        public bool IsOnLine { get; set; }
        public string IpAddress { get; set; } = default!;
        public string Support { get; set; } = default!;
        public double MemoryUsagePercentage { get; set; }
        public double CpuUsagePercentage { get; set; }
        public long Uptime { get; set; }
    }

    protected override async Task OnInitializedAsync() => await RefreshDataAsync();

    private async Task RefreshDataAsync()
    {
        var clusterClient = adminService[ClusterName];
        var client = await clusterClient.GetPveClientAsync();

        var result = await client.Cluster.Ceph.Status.Status();
        CephInstalled = result.IsSuccessStatusCode;
        string cephStatus = CephInstalled
                            ? result.Response.data.health.status
                            : null!;

        CephStatus = new()
        {
            Status = cephStatus,
            Icon = PveAdminUIHelper.CephCluster.GetIconStatus(cephStatus),
            Color = PveAdminUIHelper.CephCluster.GetColorStatus(cephStatus)
        };
        await InvokeAsync(StateHasChanged);

        var resources = await clusterClient.CachedData.GetResourcesAsync(false);

        DataUsages = ResourceUsage.Get(resources, L);
        await InvokeAsync(StateHasChanged);

        DataUsages.Add(await ResourceUsage.GetSnapshots(resources, L, clusterClient));
        await InvokeAsync(StateHasChanged);

        var status = (await client.Cluster.Status.GetAsync()).ToArray();

        var cluster = status.FirstOrDefault(a => a.Type == PveConstants.KeyApiCluster);
        if (cluster == null)
        {
            StatusInfo.Status = L["Standalone node - no cluster defined"];
            StatusInfo.Icon = "check_circle";
            StatusInfo.Color = PveAdminUIHelper.GetResourcesColorStatus(PveConstants.StatusOnline, false);
        }
        else
        {
            StatusInfo.Status = L["Cluster: {0}, Quorate: {1}",
                                  cluster.Name,
                                  cluster.Quorate == 1 ? L["Yes"] : L["No"]];

            if (status.Count(a => a.Type != PveConstants.KeyApiCluster) == cluster.Nodes)
            {
                StatusInfo.Icon = "check_circle";
                StatusInfo.Color = PveAdminUIHelper.GetResourcesColorStatus(PveConstants.StatusOnline, false);
            }
            else
            {
                StatusInfo.Icon = "warning";
                StatusInfo.Color = PveAdminUIHelper.GetResourcesColorStatus(PveConstants.StatusOffline, false);
            }
        }
        await InvokeAsync(StateHasChanged);

        var nodes = resources.Where(a => a.ResourceType == ClusterResourceType.Node).ToArray();
        Nodes = [.. status.Where(a => a.Type != PveConstants.KeyApiCluster)
                          .Select(a => new { Status = a, Node = nodes.First(b => b.Id == a.Id) })
                          .Select(a => new ClusterStatus
                          {
                              Name = a.Status.Name,
                              Id = a.Status.NodeId,
                              IsOnLine = a.Status.IsOnline,
                              Support = NodeHelper.DecodeLevelSupport(a.Status.Level).ToString(),
                              IpAddress = a.Status.IpAddress,
                              CpuUsagePercentage = a.Node.CpuUsagePercentage ,
                              MemoryUsagePercentage = a.Node.MemoryUsagePercentage,
                              Uptime = a.Node.Uptime
                          })
                          .OrderBy(a => a.Name)];

        await InvokeAsync(StateHasChanged);
    }
}
