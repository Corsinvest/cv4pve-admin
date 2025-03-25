/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using ApexCharts;
using Corsinvest.AppHero.Core.BaseUI.DataManager;
using Corsinvest.AppHero.Core.Domain.Repository;
using Corsinvest.AppHero.Core.MudBlazorUI.Style;
using Corsinvest.ProxmoxVE.Api;
using Corsinvest.ProxmoxVE.Api.Extension;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Common;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;
using Corsinvest.ProxmoxVE.Api.Shared.Utils;

namespace Corsinvest.ProxmoxVE.Admin.ClusterUsage.Components;

public partial class Costs
{
    [Parameter] public string Height { get; set; } = default!;

    [Inject] private IPveClientService PveClientService { get; set; } = default!;
    [Inject] private IReadRepository<DataVm> DataVms { get; set; } = default!;
    [Inject] private IDataGridManager<DataVmEx> DataGridManager { get; set; } = default!;
    [Inject] private IJobService JobService { get; set; } = default!;
    [Inject] private IOptionsSnapshot<Options> Options { get; set; } = default!;
    [Inject] private LayoutService LayoutService { get; set; } = default!;

    private DateRange DateRange { get; set; } = new(DateTime.Now.AddDays(-7).Date, DateTime.Now.Date);
    private MudDateRangePicker? RefPicker { get; set; } = default!;
    private PveClient PveClient { get; set; } = default!;
    private string ClusterName { get; set; } = default!;

    private ApexChartOptions<DataVmStorage> ChartOptionsStorages => new()
    {
        Chart = new()
        {
            Group = "RrdSync"
        },
        Markers = new()
        {
            Shape = ShapeEnum.Circle,
            Size = 5,
            FillOpacity = new Opacity(0.8d),
        },
        Yaxis = [
            new YAxis
            {
                Title = new AxisTitle { Text = "Usage (GB)" },
                DecimalsInFloat = 0,
            }
        ],
        Theme = new() { Mode = LayoutService.IsDarkMode ? Mode.Dark : Mode.Light }
    };

    private class DataVmEx : DataVm
    {
        public bool HasChild { get; set; } = true;
        public bool Expanded { get; set; }

        [Display(Name = "Max Storage")]
        [DisplayFormat(DataFormatString = "{0:" + FormatHelper.FormatBytes + "}")]
        public long StorageMax { get; set; }

        public double CpuCost { get; set; }
        public double MemoryCost { get; set; }
        public double StorageCost { get; set; }
        public double TotalCost => Math.Round(CpuCost + MemoryCost + StorageCost, 2);

        public IEnumerable<DataVm> Data { get; set; } = default!;

        public IEnumerable<VmRrdData> RrdData { get; set; } = default!;
    }

    protected override async Task OnInitializedAsync()
    {
        PveClient = await PveClientService.GetClientCurrentClusterAsync();

        DataGridManager.Title = L["Costs"];
        DataGridManager.DefaultSort = new() { [nameof(DataVmEx.VmId)] = false };

        DataGridManager.QueryAsync = async () =>
        {
            var moduleClusterOptions = Options.Value.Get(ClusterName);

            StorageOptions GetData(string storage) => moduleClusterOptions.Storages.First(b => b.Storage == storage);

            var data = (await DataVms.ListAsync())
                            .Where(a => a.Date >= DateRange.Start && a.Date <= DateRange.End)
                            .GroupBy(a => a.VmId)
                            .Select(a => new DataVmEx
                            {
                                Data = [.. a],
                                RrdData = a.Select(a => new VmRrdData()
                                {
                                    Time = new DateTimeOffset(a.Date).ToUnixTimeSeconds(),
                                    CpuSize = a.CpuSize,
                                    CpuUsagePercentage = a.CpuUsagePercentage,
                                    MemorySize = Convert.ToUInt64(a.MemorySize),
                                    MemoryUsage = Convert.ToUInt64(a.MemoryUsage),
                                }),

                                VmId = a.Key,
                                VmType = a.First().VmType,
                                VmName = string.Join(",", a.Select(b => b.VmName).Distinct()),
                                Node = string.Join(",", a.Select(b => b.Node).Distinct()),
                                CpuSize = a.Max(b => b.CpuSize),
                                CpuUsagePercentage = a.Average(b => b.CpuUsagePercentage),
                                MemorySize = a.Max(b => b.MemorySize),
                                MemoryUsage = Convert.ToInt64(a.Average(b => b.MemoryUsage)),

                                StorageMax = a.Max(b => b.Storages.Select(a => a.Size).Sum()),

                                CpuCost = Math.Round(a.Select(b => b.CpuSize *
                                                                    (b.CpuUsagePercentage == 0
                                                                        ? moduleClusterOptions.CostDayCpuStopped
                                                                        : moduleClusterOptions.CostDayCpuRunning))
                                                      .Sum(), 2),

                                MemoryCost = Math.Round(a.Select(b => b.MemorySize / 1024.0 / 1024.0 / 1024.0 *
                                                                        (b.CpuUsagePercentage == 0
                                                                            ? moduleClusterOptions.CostDayMemoryGbStopped
                                                                            : moduleClusterOptions.CostDayMemoryGbRunning))
                                                         .Sum(), 2),

                                StorageCost = Math.Round(a.SelectMany(b => b.Storages)
                                                          .Select(b => b.Size / 1024.0 / 1024.0 / 1024.0 *
                                                                           (b.DataVm.CpuUsagePercentage == 0
                                                                               ? GetData(b.Storage).CostDayGbStopped
                                                                               : GetData(b.Storage).CostDayGbRunning))
                                                          .Sum(), 2)
                            })
                            .ToArray();

            return data;
        };

        try
        {
            ClusterName = await PveClientService.GetCurrentClusterNameAsync();
        }
        catch { }
    }

    private async Task<IEnumerable<VmRrdData>> GetVmRrdData(DataVmEx item, RrdDataTimeFrame rrdDataTimeFrame, RrdDataConsolidation rrdDataConsolidation)
        => await PveClient.GetVmRrdDataAsync(item.Node, item.VmType, item.VmId, rrdDataTimeFrame, rrdDataConsolidation);

    private async Task OkAsync()
    {
        RefPicker!.Close();
        await DataGridManager.RefreshAsync();
    }

    private void Scan()
    {
        JobService.Schedule<Job>(a => a.ScanAsync(ClusterName), TimeSpan.FromSeconds(10));
        UINotifier.Show(L["Scan jobs started!"], UINotifierSeverity.Info);
    }
}
