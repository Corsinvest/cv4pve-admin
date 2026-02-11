using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components.ProxmoxVE.Storage;

public partial class Summary : IRefreshableData
{
    [EditorRequired, Parameter] public IClusterResourceStorage Storage { get; set; } = default!;
    protected override async Task OnInitializedAsync() => await RefreshDataAsync();
    public async Task RefreshDataAsync() => await Task.CompletedTask;
}
