using Corsinvest.ProxmoxVE.Api.Shared.Models.Storage;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components.ProxmoxVE.Storage;

public partial class Storages(IAdminService adminService) : IRefreshableData, IClusterName
{
    [EditorRequired, Parameter] public string ClusterName { get; set; } = default!;
    [Parameter] public string Style { get; set; } = default!;

    private IEnumerable<StorageItem> Items { get; set; } = default!;
    protected override async Task OnInitializedAsync() => await RefreshDataAsync();
    public async Task RefreshDataAsync()
    {
        var client = await adminService[ClusterName].GetPveClientAsync();
        Items = (await client.Storage.GetAsync()).OrderBy(a => a.Storage);
    }
}
