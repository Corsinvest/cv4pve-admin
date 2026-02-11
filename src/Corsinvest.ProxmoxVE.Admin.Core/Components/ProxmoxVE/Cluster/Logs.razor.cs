using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components.ProxmoxVE.Cluster;

public partial class Logs(IAdminService adminService) : IRefreshableData, IClusterName
{
    [EditorRequired, Parameter] public string ClusterName { get; set; } = default!;
    [Parameter] public string Style { get; set; } = default!;

    private IEnumerable<ClusterLog> Items { get; set; } = [];
    private bool IsLoading { get; set; }

    protected override async Task OnInitializedAsync() => await RefreshDataAsync();

    public async Task RefreshDataAsync()
    {
        IsLoading = true;
        await InvokeAsync(StateHasChanged);

        var client = await adminService[ClusterName].GetPveClientAsync();
        Items = await client.Cluster.Log.GetAsync(100);

        IsLoading = false;
        await InvokeAsync(StateHasChanged);
    }
}
