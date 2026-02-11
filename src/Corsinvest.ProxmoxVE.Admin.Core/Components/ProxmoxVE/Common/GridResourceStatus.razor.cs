using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components.ProxmoxVE.Common;

public partial class GridResourceStatus(IAdminService adminService) : IRefreshableData
{
    [EditorRequired, Parameter] public ClusterResourceType ResourceType { get; set; } = ClusterResourceType.Vm;
    [EditorRequired, Parameter] public string ClusterName { get; set; } = default!;
    [Parameter] public VmType VmType { get; set; } = VmType.Qemu;
    [Parameter] public EventCallback<VmType> VmTypeChanged { get; set; } = default!;
    [Parameter] public bool ShowHeader { get; set; }

    private string Type { get; set; } = default!;
    private IEnumerable<ItemStatus> Items { get; set; } = [];
    private ClusterResourceType _previousResourceType;
    private VmType _previousVmType;

    protected override async Task OnInitializedAsync()
    {
        _previousResourceType = ResourceType;
        _previousVmType = VmType;
        await RefreshDataAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        if (_previousResourceType != ResourceType || _previousVmType != VmType)
        {
            _previousResourceType = ResourceType;
            _previousVmType = VmType;
            await RefreshDataAsync();
        }
    }

    private async Task<List<ItemStatus>> GetItemsAsync(string clusterName)
    {
        var data = (await adminService[clusterName].CachedData.GetResourcesAsync(false))
                        .Where(a => a.ResourceType == ResourceType)
                        .Where(a => a.VmType == VmType, ResourceType == ClusterResourceType.Vm);

        Type = data.FirstOrDefault()?.Type!;

        return [.. data.GroupBy(a => new { a.Status })
                       .Select(a => new ItemStatus
                       {
                           Count = a.Count(),
                           Status = a.Key.Status
                       })];
    }

    public async Task RefreshDataAsync()
    {
        var items = new List<ItemStatus>();

        if (string.IsNullOrEmpty(ClusterName))
        {
            foreach (var item in adminService)
            {
                items.AddRange(await GetItemsAsync(item.Settings.Name));
            }
        }
        else
        {
            items.AddRange(await GetItemsAsync(ClusterName));
        }

        Items = [.. items.GroupBy(a => new { a.Status })
                         .Select(a => new ItemStatus
                         {
                             Count = a.Sum(b => b.Count),
                             Status = a.Key.Status,
                             Icon = PveAdminUIHelper.Icons.GetResourceStatus(a.Key.Status,false,true),
                             Color = PveAdminUIHelper.GetResourcesColorStatus(a.Key.Status, false)
                         })
                         .OrderBy(a => a.Status)];

        await InvokeAsync(StateHasChanged);
    }

    private string GetText()
        => ResourceType switch
        {
            ClusterResourceType.Vm => VmType switch
            {
                VmType.Qemu => L["Virtual Machines"],
                VmType.Lxc => L["LXC Container"],
                _ => string.Empty
            },
            _ => ResourceType.ToString()
        };
}
