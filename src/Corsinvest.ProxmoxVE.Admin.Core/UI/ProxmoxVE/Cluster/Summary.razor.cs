/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.Domain.Contracts;
using Corsinvest.AppHero.Core.MudBlazorUI.Style;
using Corsinvest.AppHero.Core.Security.Auth.Permissions;
using Corsinvest.ProxmoxVE.Admin.Core.Models;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;
using Corsinvest.ProxmoxVE.Api.Shared.Utils;

namespace Corsinvest.ProxmoxVE.Admin.Core.UI.ProxmoxVE.Cluster;

public partial class Summary : IRefreshable
{
    [Parameter][Category("Header")] public bool FixedHeader { get; set; }
    [Parameter][Category("Footer")] public bool FixedFooter { get; set; }
    [Parameter] public PermissionsRead Permissions { get; set; } = default!;
    [EditorRequired][Parameter] public Func<Task<IEnumerable<ClusterStatus>>> GetStatus { get; set; } = default!;
    [EditorRequired][Parameter] public Func<Task<IEnumerable<ClusterResource>>> GetResources { get; set; } = default!;
    [EditorRequired][Parameter] public Func<Task<string?>> GetCephStatus { get; set; } = default!;

    [Inject] private LayoutService LayoutService { get; set; } = default!;
    [Inject] private IDataGridManager<ClusterStatusEx> DataGridManager { get; set; } = default!;

    private ItemStatus CephStatus { get; set; } = default!;
    private bool CephInstalled { get; set; }

    public async Task RefreshAsync() => await DataGridManager.RefreshAsync();

    private class DataUsage : ResourceUsage
    {
        public List<string> Colors { get; set; } = default!;
    }

    private class VmStatus : ItemStatus
    {
        public VmType Type { get; set; }
    }

    private class ItemStatus
    {
        public string Status { get; set; } = default!;
        public int Count { get; set; }
        public string Icon { get; set; } = default!;
        public Color Color { get; set; }
    }

    private List<VmStatus> VmsStatus { get; set; } = [];
    private List<DataUsage> DataUsages { get; set; } = [];
    private List<ItemStatus> NodeHealts { get; set; } = [];
    private ItemStatus StatusInfo { get; set; } = new();

    private class ClusterStatusEx
    {
        public string Name { get; set; } = default!;
        public int? Id { get; set; }
        public bool IsOnLine { get; set; }
        public string IpAddress { get; set; } = default!;
        public string Support { get; set; } = default!;

        [Display(Name = "Memory Usage")]
        [DisplayFormat(DataFormatString = "{0:P1}")]
        public double MemoryUsagePercentage { get; set; }

        [Display(Name = "CPU Usage")]
        [DisplayFormat(DataFormatString = "{0:P1}")]
        public double CpuUsagePercentage { get; set; }

        [DisplayFormat(DataFormatString = "{0:" + FormatHelper.FormatUptimeUnixTime + "}")]
        public long Uptime { get; set; }
    }

    protected override void OnInitialized()
    {
        DataGridManager.Title = L["Disk"];
        DataGridManager.DefaultSort = new() { [nameof(ClusterStatusEx.Name)] = false };
        DataGridManager.QueryAsync = GetStatusInt;
    }

    private ApexCharts.ApexChartOptions<ApexCharts.GaugeValue> GetOptions(DataUsage item)
        => new()
        {
            Colors = item.Colors,
            Chart = new() { Background = "trasparent" },
            Theme = new() { Mode = LayoutService.IsDarkMode ? ApexCharts.Mode.Dark : ApexCharts.Mode.Light }
        };

    private async Task<IEnumerable<ClusterStatusEx>> GetStatusInt()
    {
        var cephStatus = await GetCephStatus();
        CephStatus = new ItemStatus
        {
            Status = cephStatus!,
            Icon = PveBlazorHelper.CephCluster.GetIconStatus(cephStatus!),
            Color = PveBlazorHelper.CephCluster.GetColorStatus(cephStatus!)
        };
        CephInstalled = cephStatus != null;

        var resources = (await GetResources()).ToList();

        DataUsages.Clear();
        foreach (var item in ResourceUsage.GetUsages(resources, L))
        {
            DataUsages.Add(new()
            {
                Name = item.Name,
                Usage = item.Usage,
                Info = item.Info,
                Colors = [item.Color]
            });
        }

        VmsStatus = [.. resources.Where(a => a.ResourceType == ClusterResourceType.Vm)
                                 .GroupBy(a => new { a.VmType, a.Status })
                                 .Select(a => new VmStatus
                                 {
                                    Type = a.Key.VmType,
                                    Count = a.Count(),
                                    Status = a.Key.Status,
                                    Icon = PveBlazorHelper.Icons.GetStatus(a.Key.Status),
                                    Color = PveBlazorHelper.GetColorStatus(a.Key.Status)
                                 })];

        var nodes = resources.Where(a => a.ResourceType == ClusterResourceType.Node).ToArray();
        NodeHealts = [.. nodes.GroupBy(a => a.Status).Select(a => new ItemStatus
        {
            Count = a.Count(),
            Status = a.Key,
            Icon = PveBlazorHelper.Icons.GetStatus(a.Key),
            Color = PveBlazorHelper.GetColorStatus(a.Key)
        })];

        var status = (await GetStatus()).ToArray();
        var cluster = status.FirstOrDefault(a => a.Type == PveConstants.KeyApiCluster);
        if (cluster == null)
        {
            StatusInfo.Status = L["Standalone node - no cluster defined"];
            StatusInfo.Icon = Icons.Material.Filled.CheckCircle;
            StatusInfo.Color = PveBlazorHelper.GetColorStatus(PveConstants.StatusOnline);
        }
        else
        {
            StatusInfo.Status = L["Cluster: {0}, Quorate: {1}",
                                  cluster.Name,
                                  cluster.Quorate == 1 ? L["Yes"] : L["No"]];

            if (status.Count(a => a.Type != PveConstants.KeyApiCluster) == cluster.Nodes)
            {
                StatusInfo.Icon = Icons.Material.Filled.CheckCircle;
                StatusInfo.Color = PveBlazorHelper.GetColorStatus(PveConstants.StatusOnline);
            }
            else
            {
                StatusInfo.Icon = Icons.Material.Filled.Warning;
                StatusInfo.Color = PveBlazorHelper.GetColorStatus(PveConstants.StatusOffline);
            }
        }

        StateHasChanged();

        return status.Where(a => a.Type != PveConstants.KeyApiCluster)
                     .Select(a => new { Status = a, Node = nodes.First(b => b.Id == a.Id) })
                     .Select(a => new ClusterStatusEx
                     {
                         Name = a.Status.Name,
                         Id = a.Status.NodeId,
                         IsOnLine = a.Status.IsOnline,
                         Support = NodeHelper.DecodeLevelSupport(a.Status.Level).ToString(),
                         IpAddress = a.Status.IpAddress,
                         CpuUsagePercentage = a.Node.CpuUsagePercentage,
                         MemoryUsagePercentage = a.Node.MemoryUsagePercentage,
                         Uptime = a.Node.Uptime,
                     })
                     .OrderBy(a => a.Name);
    }
}
