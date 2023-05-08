/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using ApexCharts;
using Corsinvest.AppHero.Core.BaseUI.DataManager;
using Corsinvest.AppHero.Core.Domain.Repository;
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

    private DateRange DateRange { get; set; } = new(DateTime.Now.AddDays(-7).Date, DateTime.Now.Date);
    private MudDateRangePicker? RefPicker { get; set; } = default!;
    private PveClient PveClient { get; set; } = default!;

    private static ApexChartOptions<DataVmStorage> ChartOptionsStorages => new()
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
        Yaxis = new() {
            new YAxis
            {
                Title = new AxisTitle { Text = "Usage (GB)" },
                DecimalsInFloat = 0,
            }
        }
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
        PveClient = await PveClientService.GetClientCurrentCluster();

        DataGridManager.Title = L["Costs"];
        DataGridManager.DefaultSort = new() { [nameof(DataVmEx.VmId)] = false };

        DataGridManager.QueryAsync = async () =>
        {
            var moduleClusterOptions = Options.Value.Get(await PveClientService.GetCurrentClusterName());

            StorageOptions GetData(string storage) => moduleClusterOptions.Storages.First(b => b.Storage == storage);

            var data = (await DataVms.ListAsync())
                            .Where(a => a.Date >= DateRange.Start && a.Date <= DateRange.End)
                            .GroupBy(a => a.VmId)
                            .Select(a => new DataVmEx
                            {
                                Data = a.ToList(),
                                RrdData = a.Select(a => new VmRrdData()
                                {
                                    Time = new DateTimeOffset(a.Date).ToUnixTimeSeconds(),
                                    CpuSize = a.CpuSize,
                                    CpuUsagePercentage = a.CpuUsagePercentage,
                                    MemorySize = a.MemorySize,
                                    MemoryUsage = a.MemoryUsage,
                                }),

                                VmId = a.Key,
                                VmName = string.Join(",", a.Select(a => a.VmName).Distinct()),
                                Node = string.Join(",", a.Select(a => a.Node).Distinct()),
                                CpuSize = a.Max(a => a.CpuSize),
                                CpuUsagePercentage = a.Average(a => a.CpuUsagePercentage),
                                MemorySize = a.Max(a => a.MemorySize),
                                MemoryUsage = Convert.ToInt64(a.Average(a => a.MemoryUsage)),

                                StorageMax = a.Max(a => a.Storages.Sum(a => a.Size)),

                                CpuCost = Math.Round(a.Select(a => a.CpuSize *
                                                                    (a.CpuUsagePercentage == 0
                                                                        ? moduleClusterOptions.CostDayCpuStopped
                                                                        : moduleClusterOptions.CostDayCpuRunning))
                                                      .Sum(), 2),

                                MemoryCost = Math.Round(a.Select(a => a.MemorySize / 1024.0 / 1024.0 / 1024.0 *
                                                                        (a.CpuUsagePercentage == 0
                                                                            ? moduleClusterOptions.CostDayMemoryGbStopped
                                                                            : moduleClusterOptions.CostDayMemoryGbRunning))
                                                         .Sum(), 2),

                                StorageCost = Math.Round(a.SelectMany(a => a.Storages)
                                                           .Select(a => a.Size / 1024.0 / 1024.0 / 1024.0 *
                                                                            (a.DataVm.CpuUsagePercentage == 0
                                                                                ? GetData(a.Storage).CostDayGbStopped
                                                                                : GetData(a.Storage).CostDayGbRunning))
                                                           .Sum(), 2)
                            })
                            .ToArray();

            return data;
        };
    }

    private async Task<IEnumerable<VmRrdData>> GetVmRrdData(DataVmEx item, RrdDataTimeFrame rrdDataTimeFrame, RrdDataConsolidation rrdDataConsolidation)
        => await PveClient.GetVmRrdData(await PveClient.GetVm(item.VmId), rrdDataTimeFrame, rrdDataConsolidation);

    private async Task Ok()
    {
        RefPicker!.Close();
        await DataGridManager.Refresh();
    }

    private async Task Scan()
    {
        var clusterName = await PveClientService.GetCurrentClusterName();
        JobService.Schedule<Job>(a => a.Scan(clusterName), TimeSpan.FromSeconds(10));
        UINotifier.Show(L["Scan jobs started!"], UINotifierSeverity.Info);
    }
}