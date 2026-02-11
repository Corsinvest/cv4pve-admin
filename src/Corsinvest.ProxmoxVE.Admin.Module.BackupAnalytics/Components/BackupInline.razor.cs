using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Node;

namespace Corsinvest.ProxmoxVE.Admin.Module.BackupAnalytics.Components;

public partial class BackupInline(IAdminService adminService) : IClusterName
{
    [CascadingParameter(Name = nameof(ClusterName))] public string ClusterName { get; set; } = default!;

    private IEnumerable<NodeStorageContent> Items { get; set; } = [];
    private bool IsLoading { get; set; }

    private IEnumerable<string> PropertiesName { get; } =
    [
        nameof(NodeStorageContent.Storage),
        nameof(NodeStorageContent.VmId),
        nameof(NodeStorageContent.FileName),
        nameof(NodeStorageContent.Size),
        nameof(NodeStorageContent.Creation),
        nameof(NodeStorageContent.Format),
        nameof(NodeStorageContent.Verified),
        nameof(NodeStorageContent.Encrypted)
    ];

    private IEnumerable<GroupDescriptor> Groups { get; set; } = [];

    protected override async Task OnInitializedAsync()
    {
        Groups =
        [
            new ()
            {
                Title = L["Storage"],
                Property = nameof(NodeStorageContent.Storage)
            },
            new ()
            {
                Title = L["Vm Id"],
                Property = nameof(NodeStorageContent.VmId)
            }
        ];

        Items = await GetBackupsInline();
    }

    private async Task<IEnumerable<NodeStorageContent>> GetBackupsInline()
    {
        IsLoading = true;
        var clusterClient = adminService[ClusterName];
        var client = await clusterClient.GetPveClientAsync();

        var ret = new List<NodeStorageContent>();

        //TODO se ci sono tanti nodi e vm diventa lento?
        foreach (var node in (await clusterClient.CachedData.GetResourcesAsync(false)).Where(a => a.ResourceType == ClusterResourceType.Node && a.IsOnline))
        {
            ret.AddRange(await client.Nodes[node.Node].GetBackupsInAllStoragesAsync());
        }

        IsLoading = false;

        return [.. ret.Distinct()];
    }
}
