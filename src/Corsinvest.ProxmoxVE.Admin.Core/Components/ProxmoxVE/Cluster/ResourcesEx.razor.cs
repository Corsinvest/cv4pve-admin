using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;
using Mapster;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components.ProxmoxVE.Cluster;

public partial class ResourcesEx(IAdminService adminService) : IRefreshableData,
                                                               IClusterNamesParameters,
                                                               IDisposable
{
    [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object>? AdditionalAttributes { get; set; }
    [Parameter] public IEnumerable<string> ClusterNames { get; set; } = [];
    [Parameter] public EventCallback<IEnumerable<string>> ClusterNamesChanged { get; set; } = default!;
    [Parameter] public string? Style { get; set; }
    [Parameter] public bool UseProgressBarPercentage { get; set; } = true;
    [Parameter] public bool DescriptionAsLink { get; set; } = true;
    [Parameter] public DataGridSettings DataGridSettings { get; set; } = new();
    [Parameter] public EventCallback<DataGridSettings> DataGridSettingsChanged { get; set; } = default!;
    [Parameter] public Func<ClusterResourceEx, string, bool>? Filter { get; set; }
    [Parameter] public bool ShowSnapshotSize { get; set; }
    [Parameter] public bool ShowOsInfo { get; set; }
    [Parameter] public RenderFragment<ClusterResourceEx> Template { get; set; } = default!;
    [Parameter] public bool ShowLoading { get; set; }
    [Parameter] public bool AvailableCommands { get; set; }
    [Parameter] public ResourcesExPropertyIconStatus PropertyIconStatus { get; set; } = ResourcesExPropertyIconStatus.None;
    [Parameter] public ResourceColumnIconStatus IconStatus { get; set; } = ResourceColumnIconStatus.IconAndText;
    [Parameter] public ClusterResourceType ResourceType { get; set; } = ClusterResourceType.All;
    [Parameter] public RenderFragment? TemplateToolbar { get; set; }
    [Parameter] public DataGridSelectionMode SelectionMode { get; set; } = DataGridSelectionMode.Single;
    [Parameter] public IList<ClusterResourceEx> SelectedItems { get; set; } = [];
    [Parameter] public EventCallback<IList<ClusterResourceEx>> SelectedItemsChanged { get; set; }
    [Parameter] public HashSet<string> PickableColumns { get; set; } = [];
    [Parameter] public bool AllowColumnPicking { get; set; } = true;
    [Parameter] public bool AllowSearch { get; set; } = true;

    private int _refreshInterval;
    [Parameter]
    public int RefreshInterval
    {
        get => _refreshInterval;
        set
        {
            if (_refreshInterval != value)
            {
                _refreshInterval = value;
                StartTimer();
            }
        }
    }

    private RadzenDataGrid<ClusterResourceEx>? DataGridRef { get; set; }
    private List<ClusterResourceEx> Items { get; set; } = [];
    private Dictionary<string, string> TagStyleColorMaps { get; set; } = [];

    private bool IsLoading { get; set; }
    private bool IsCalculateSnapshotSize { get; set; }
    private bool IsGetOsInfo { get; set; }
    private bool AllowCalculateSnapshotSize { get; set; }
    private string CommandsWidth { get; set; } = "100px";
    private bool HasColumnForVm => ResourceType.HasFlag(ClusterResourceType.Vm) || ResourceType.HasFlag(ClusterResourceType.All);
    private bool HasColumnForNode => ResourceType.HasFlag(ClusterResourceType.Node) || ResourceType.HasFlag(ClusterResourceType.All);
    private bool HasColumnForStorage => ResourceType.HasFlag(ClusterResourceType.Storage) || ResourceType.HasFlag(ClusterResourceType.All);

    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private bool _inGetGroupHeader;
    private Timer? _timer;

    protected override void OnParametersSet() => StartTimer();
    protected override async Task OnInitializedAsync() => await RefreshDataAsync();

    public async Task RefreshDataAsync()
    {
        if (!await _refreshLock.WaitAsync(0)) { return; }
        IsLoading = true;

        try
        {
            await RefreshDataAsyncInt();
        }
        finally
        {
            IsLoading = false;
            _refreshLock?.Release();
        }
    }

    private async Task RefreshDataAsyncInt()
    {
        AllowCalculateSnapshotSize = false;

        foreach (var clusterClient in adminService.Where(a => ClusterNames.Contains(a.Settings.Name), ClusterNames.Any()))
        {
            var clusterName = clusterClient.Settings.Name;
            PveClient? client = null;

            try
            {
                client = await clusterClient.GetPveClientAsync();
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Failed to connect to cluster {ClusterName}", clusterName);
            }

            if (client == null) { continue; }

            TagStyleColorMaps[clusterName] = await clusterClient.CachedData.GetTagStyleColorMapAsync(false);

            var items = (await clusterClient.CachedData.GetResourcesAsync(false))
                                .AsQueryable()
                                .ProjectToType<ClusterResourceEx>()
                                .ToList()
                                .Select(item =>
                                {
                                    item.ClusterName = clusterName;
                                    item.Link = PveAdminHelper.GetPveUrl(client.BaseAddress, item.Id);
                                    return item;
                                })
                                .Where(a => Filter!(a, clusterName), Filter != null)
                                .ToList();

            //remove items that the user has not permission to see
            var allowedItems = new List<ClusterResourceEx>();
            foreach (var item in items)
            {
                if (await PermissionService.HasAsync(clusterName, item))
                {
                    allowedItems.Add(item);
                }
            }
            items = allowedItems;

            #region Sync data
            // Use Dictionary for O(n) performance instead of O(n²)
            var existingItemsDict = Items.ToDictionary(a => (a.Id, a.ClusterName));

            // Add or update items
            foreach (var item in items)
            {
                if (existingItemsDict.TryGetValue((item.Id, item.ClusterName), out var row))
                {
                    item.Adapt(row);
                }
                else
                {
                    Items.Add(item);
                }
            }

            // Remove old items
            var newItemsKeys = items.Select(a => (a.Id, a.ClusterName)).ToHashSet();
            foreach (var item in Items.Where(a => !newItemsKeys.Contains((a.Id, a.ClusterName))).ToList())
            {
                Items.Remove(item);
            }
            #endregion

            if (ShowOsInfo)
            {
                IsGetOsInfo = true;
                foreach (var item in Items.Where(a => a.ResourceType == ClusterResourceType.Vm))
                {
                    item.Set(await clusterClient.CachedData.GetVmOsInfoAsync(item, false));
                    await InvokeAsync(StateHasChanged);
                }
                IsGetOsInfo = false;
            }

            #region Snapshot
            if (clusterClient.Settings.AllowCalculateSnapshotSize && ShowSnapshotSize) { AllowCalculateSnapshotSize = true; }

            if (AllowCalculateSnapshotSize)
            {
                IsCalculateSnapshotSize = true;
                await InvokeAsync(StateHasChanged);

                var disks = await clusterClient.CachedData.GetDisksInfoAsync(false);

                foreach (var item in Items)
                {
                    item.SnapshotsSize = item.ResourceType switch
                    {
                        ClusterResourceType.Node => DiskInfoHelper.CalculateSnapshots(item.Node, disks, false),
                        ClusterResourceType.Vm => DiskInfoHelper.CalculateSnapshots(item.Node, item.VmId, disks, false),
                        ClusterResourceType.Storage => DiskInfoHelper.CalculateSnapshots(item.Node, item.Storage, disks, false),
                        ClusterResourceType.Unknown or ClusterResourceType.Pool or ClusterResourceType.Sdn or ClusterResourceType.All => 0,
                        _ => 0
                    };

                    item.SnapshotsReplicationSize = item.ResourceType switch
                    {
                        ClusterResourceType.Node => DiskInfoHelper.CalculateSnapshots(item.Node, disks, true),
                        ClusterResourceType.Vm => DiskInfoHelper.CalculateSnapshots(item.Node, item.VmId, disks, true),
                        ClusterResourceType.Storage => DiskInfoHelper.CalculateSnapshots(item.Node, item.Storage, disks, true),
                        ClusterResourceType.Unknown or ClusterResourceType.Pool or ClusterResourceType.Sdn or ClusterResourceType.All => 0,
                        _ => 0
                    };
                }

                IsCalculateSnapshotSize = false;
            }
            #endregion
        }

        Items = [.. Items];

        await InvokeAsync(StateHasChanged);
    }

    public async Task ReloadSettingsAsync() => await DataGridRef!.ReloadSettings(true);
    public void SaveSettings() => DataGridRef?.SaveSettings();

    private bool IsPickable(string propertyName) => PickableColumns.Count == 0 || PickableColumns.Contains(propertyName);

    private ResourceColumnIconStatus GetIconStatus(string propertyName)
        => (PropertyIconStatus, propertyName) switch
        {
            (ResourcesExPropertyIconStatus.Type, nameof(ClusterResource.Type)) => IconStatus,
            (ResourcesExPropertyIconStatus.Status, nameof(ClusterResource.Status)) => IconStatus,
            (ResourcesExPropertyIconStatus.Description, nameof(ClusterResource.Description)) => IconStatus,
            (ResourcesExPropertyIconStatus.VmId, nameof(ClusterResource.VmId)) => IconStatus,
            (ResourcesExPropertyIconStatus.Node, nameof(ClusterResource.Node)) => IconStatus,
            (ResourcesExPropertyIconStatus.Storage, nameof(ClusterResource.Storage)) => IconStatus,
            _ => ResourceColumnIconStatus.None
        };

    private string GetGroupHeader(Group group)
    {
        if (_inGetGroupHeader) { return string.Empty; }
        _inGetGroupHeader = true;

        var sb = new List<string>();

        void GetUsages(IEnumerable<ClusterResource> items)
            => sb.AddRange(ResourceUsage.Get(items, L)
                 .Select(a => $"<strong>{a.Name}</strong>: {a.Usage}%"));

        if (group.Data.Count > 0)
        {
            var clusterName = group.Data.Items!.Cast<ClusterResourceEx>().FirstOrDefault()!.ClusterName;

            if (group.GroupDescriptor!.Property == nameof(IClusterName.ClusterName))
            {
                GetUsages(Items.Where(a => a.ClusterName == clusterName));
            }
            else if (group.GroupDescriptor.Property == nameof(INode.Node))
            {
                GetUsages(Items.Where(a => a.ClusterName == clusterName && a.Node == group.Data.Key));
            }
        }

        _inGetGroupHeader = false;
        return sb.JoinAsString(" ");
    }

    private void StartTimer()
    {
        StopTimer();

        if (RefreshInterval > 0)
        {
            _timer = new Timer(async _ => await RefreshDataAsync(), null, RefreshInterval * 1000, RefreshInterval * 1000);
        }
    }

    private void StopTimer()
    {
        _timer?.Change(Timeout.Infinite, 0);
        _timer?.Dispose();
        _timer = null;
    }

    private async Task OnDataGridSettingsChanged() => await DataGridSettingsChanged.InvokeAsync(DataGridSettings);

    public void Dispose()
    {
        StopTimer();
        _refreshLock?.Dispose();
        GC.SuppressFinalize(this);
    }
}
