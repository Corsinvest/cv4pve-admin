using Corsinvest.ProxmoxVE.Api.Shared.Models.Node;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components.ProxmoxVE.Nodes;

public partial class DiskSmarts(IAdminService adminService) : IRefreshableData, INode, IClusterName
{
    [EditorRequired, Parameter] public string ClusterName { get; set; } = default!;
    [Parameter] public string Node { get; set; } = default!;
    [Parameter] public string Disk { get; set; } = default!;
    [Parameter] public string Style { get; set; } = default!;

    private bool IsLoading { get; set; }
    private NodeDiskSmart Data { get; set; } = default!;

    protected override async Task OnInitializedAsync() => await RefreshDataAsync();
    public async Task RefreshDataAsync()
    {
        IsLoading = true;
        var client = await adminService[ClusterName].GetPveClientAsync();
        var data = await client.GetDiskSmart(Node, Disk);
        data.Text = (data.Text ?? string.Empty).Replace("\n", "<br>");
        Data = data;
        IsLoading = false;
    }
}
