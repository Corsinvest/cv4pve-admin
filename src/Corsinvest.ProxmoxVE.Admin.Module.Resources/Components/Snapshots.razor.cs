using Corsinvest.ProxmoxVE.Api.Shared.Utils;

namespace Corsinvest.ProxmoxVE.Admin.Module.Resources.Components;

public partial class Snapshots(IAdminService adminService) : IClusterName, IRefreshableData
{
    [CascadingParameter(Name = nameof(ClusterName))] public string ClusterName { get; set; } = default!;

    private IEnumerable<Data> Items { get; set; } = [];
    private IEnumerable<string> SelectedHosts { get; set; } = [];
    private IEnumerable<Storage> SelectedStorages { get; set; } = [];
    private IEnumerable<long> SelectedVmIds { get; set; } = [];
    private bool AllowCalculateSnapshotSize { get; set; }
    private bool IsLoading { get; set; }
    private IEnumerable<Storage> Storages { get; set; } = [];
    private IEnumerable<long> VmIds { get; set; } = [];
    private IEnumerable<Data> ItemsCharts { get; set; } = [];
    private RadzenDataGrid<Data> DataGridRef { get; set; } = default!;

    private record Storage(string Host, string Space);

    private record Data(string Host,
                        string Type,
                        long VmId,
                        string Space,
                        string Disk,
                        string Name,
                        bool ForReplication,
                        double SnapshotSize,
                        DateTime Date);

    protected override async Task OnInitializedAsync()
    {
        var clusterClient = adminService[ClusterName];
        AllowCalculateSnapshotSize = clusterClient.Settings.AllowCalculateSnapshotSize;

        await RefreshDataAsync();
    }

    //protected override async Task OnAfterRenderAsync(bool firstRender)
    //{
    //    if (firstRender) {
    //        await RefreshDataAsync();
    //    }
    //}

    private void OnRender(DataGridRenderEventArgs<Data> args)
    {
        if (args.FirstRender)
        {
            DataGridRef.Groups.Add(new()
            {
                Title = L["Host"],
                Property = nameof(Data.Host)
            });

            if (AllowCalculateSnapshotSize)
            {
                DataGridRef.Groups.Add(new()
                {
                    Title = L["Space"],
                    Property = nameof(Data.Space)
                });
            }

            DataGridRef.Groups.Add(new()
            {
                Title = L["Vm Id"],
                Property = nameof(Data.VmId)
            });

            if (AllowCalculateSnapshotSize)
            {
                DataGridRef.Groups.Add(new()
                {
                    Title = L["Disk"],
                    Property = nameof(Data.Disk)
                });
            }
        }

        StateHasChanged();
    }

    public async Task RefreshDataAsync()
    {
        IsLoading = true;
        Items = [];

        await InvokeAsync(StateHasChanged);

        var clusterClient = adminService[ClusterName];
        if (AllowCalculateSnapshotSize)
        {
            Items = (await clusterClient.CachedData.GetDisksInfoAsync(false))
                        .SelectMany(a => a.Snapshots, (a, b) => new { Disk = a, Snapshot = b })
                        .Select(a => new Data(a.Disk.Host,
                                              a.Disk.Type,
                                              a.Disk.VmId,
                                              a.Disk.SpaceName,
                                              a.Disk.Disk,
                                              a.Snapshot.Name,
                                              a.Snapshot.Replication,
                                              a.Snapshot.Size,
                                              a.Snapshot.Date));
        }
        else
        {
            var vms = (await clusterClient.CachedData.GetResourcesAsync(false))
                        .Where(a => a.ResourceType == ClusterResourceType.Vm
                                    && !a.IsTemplate)
                        .ToList();

            var items = new List<Data>();
            foreach (var vm in vms)
            {
                var snapshots = (await clusterClient.CachedData.GetSnapshotsAsync(vm.Node, vm.VmType, vm.VmId, false))
                              .Select(a => new Data(vm.Node,
                                                    string.Empty,
                                                    vm.VmId,
                                                    string.Empty,
                                                    string.Empty,
                                                    a.Name,
                                                    false,
                                                    0,
                                                    a.Date))
                              .ToList();

                items.AddRange(snapshots);
            }

            Items = items;
        }

        IsLoading = false;
        await InvokeAsync(StateHasChanged);
    }

    private MarkupString GetGroupTitle(Group group)
    {
        var ret = $"<strong>{group.GroupDescriptor!.GetTitle()}</strong>: {group.Data.Key}({group.Data.Count})";
        if (AllowCalculateSnapshotSize)
        {
            ret += $"- <strong>{L["Size"]}</strong>: {FormatHelper.FromBytes(group.Data.Items!.Cast<Data>().Sum(a => a.SnapshotSize))}";
        }

        return (MarkupString)ret;
    }

    private record ChatItem(string VmId, DateTime Date, double SnapshotSize);

    private void LoadStorages()
        => Storages = [.. Items.Where(a => SelectedHosts.Contains(a.Host))
                           .Select(a => new Storage(a.Host, a.Space))
                           .Distinct()
                           .OrderBy(a => a.Host)
                           .ThenBy(a => a.Space)];

    private void LoadVmIds()
    {
        SelectedStorages ??= [];
        SelectedVmIds = [];
        VmIds = [.. Items.Where(a => SelectedStorages.Any(b => b.Host == a.Host && b.Space == a.Space))
                     .Select(a => a.VmId)
                     .Distinct()
                     .Order()];
    }

    private void LoadDataCharts()
    {
        SelectedVmIds ??= [];
        ItemsCharts = Items.Where(a => SelectedStorages.Any(b => b.Host == a.Host
                                                                            && b.Space == a.Space)
                                                                            && SelectedVmIds.Contains(a.VmId));
    }
}
