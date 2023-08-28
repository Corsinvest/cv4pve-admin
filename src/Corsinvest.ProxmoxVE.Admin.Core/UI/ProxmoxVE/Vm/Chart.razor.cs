/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using ApexCharts;
using Corsinvest.AppHero.Core.MudBlazorUI.Style;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;

namespace Corsinvest.ProxmoxVE.Admin.Core.UI.ProxmoxVE.Vm;

partial class Chart
{
    [Parameter] public IEnumerable<VmRrdData> RrdData { get; set; } = default!;
    [Parameter] public VmChart VmChart { get; set; } = default!;

    [Inject] private LayoutService LayoutService { get; set; } = default!;

    private string Name1 { get; set; } = default!;
    private string Name2 { get; set; } = default!;
    private string Title { get; set; } = default!;
    private string Icon { get; set; } = default!;
    private IEnumerable<Data> Items { get; set; } = new List<Data>();
    private ApexChartOptions<Data> ChartOptions { get; set; } = new();
    private SeriesType SeriesType { get; set; } = SeriesType.Line;

    private ApexChartOptions<Data> ChartOptionsCpu => new()
    {
        Chart = new()
        {
            Group = "VmChart",
            Background = "trasparent"
        },
        Markers = new()
        {
            Shape = ShapeEnum.Circle,
            Size = 5,
            FillOpacity = new Opacity(0.8d),
        },
        Yaxis = new()
        {
            new YAxis
            {
                Title = new AxisTitle { Text = L["CPU Size"] },
                DecimalsInFloat = 0
            },
            new YAxis
            {
                Title = new AxisTitle { Text = L["CPU Usage %"] },
                DecimalsInFloat = 1,
                Opposite = true
            }
        },
        Theme = new() { Mode = LayoutService.IsDarkMode ? Mode.Dark : Mode.Light }
    };

    private ApexChartOptions<Data> ChartOptionsSize => new()
    {
        Chart = new()
        {
            Group = "VmChart",
            Background = "trasparent"
        },
        Markers = new()
        {
            Shape = ShapeEnum.Circle,
            Size = 5,
            FillOpacity = new Opacity(0.8d),
        },
        Yaxis = new()
        {
            new YAxis
            {
                Title = new AxisTitle { Text = L["Usage (GB)"] },
                DecimalsInFloat = 1,
            }
        },
        Theme = new() { Mode = LayoutService.IsDarkMode ? Mode.Dark : Mode.Light }
    };

    private ApexChartOptions<Data> ChartOptionsKb => new()
    {
        Chart = new()
        {
            Group = "VmChart",
            Background = "trasparent"
        },
        Markers = new()
        {
            Shape = ShapeEnum.Circle,
            Size = 5,
            FillOpacity = new Opacity(0.8d),
        },
        Yaxis = new()
        {
            new YAxis
            {
                Title = new AxisTitle { Text = L["Usage (Kb)"] },
                DecimalsInFloat = 1,
            }
        },
        Theme = new() { Mode = LayoutService.IsDarkMode ? Mode.Dark : Mode.Light }
    };

    private class Data
    {
        public DateTime Date { get; set; }
        public decimal Value1 { get; set; }
        public decimal Value2 { get; set; }
    }

    protected override void OnInitialized()
    {
        switch (VmChart)
        {
            case VmChart.Cpu:
                Title = L["CPU"];
                Icon = PveBlazorHelper.Icons.Cpu;
                SeriesType = SeriesType.Line;
                ChartOptions = ChartOptionsCpu;
                Name1 = L["Size"];
                Name2 = L["Usage (%)"];
                Items = RrdData.Select(a => new Data()
                {
                    Date = a.TimeDate,
                    Value1 = a.CpuSize,
                    Value2 = Convert.ToDecimal(a.CpuUsagePercentage * 100),
                });
                break;

            case VmChart.DiskUsage:
                Title = L["Disk Usage"];
                Icon = PveBlazorHelper.Icons.Storage;
                SeriesType = SeriesType.Area;
                ChartOptions = ChartOptionsSize;
                Name1 = L["Size (GB)"];
                Name2 = L["Usage (GB)"];
                Items = RrdData.Select(a => new Data()
                {
                    Date = a.TimeDate,
                    Value1 = Convert.ToDecimal(a.DiskSize / 1024.0 / 1024 / 1024),
                    Value2 = Convert.ToDecimal(a.DiskUsage / 1024.0 / 1024 / 1024),
                });
                break;

            case VmChart.DiskIO:
                Title = L["Disk IO"];
                Icon = PveBlazorHelper.Icons.Storage;
                SeriesType = SeriesType.Area;
                ChartOptions = ChartOptionsKb;
                Name1 = L["Read (kB)"];
                Name2 = L["Write (kB)"];
                Items = RrdData.Select(a => new Data()
                {
                    Date = a.TimeDate,
                    Value1 = Convert.ToDecimal(a.DiskRead / 1024.0),
                    Value2 = Convert.ToDecimal(a.DiskWrite / 1024.0),
                });
                break;

            case VmChart.Memory:
                Title = L["Memory"];
                Icon = PveBlazorHelper.Icons.Memory;
                SeriesType = SeriesType.Area;
                ChartOptions = ChartOptionsSize;
                Name1 = L["Size (GB)"];
                Name2 = L["Usage (GB)"];
                Items = RrdData.Select(a => new Data()
                {
                    Date = a.TimeDate,
                    Value1 = Convert.ToDecimal(a.MemorySize / 1024.0 / 1024 / 1024),
                    Value2 = Convert.ToDecimal(a.MemoryUsage / 1024.0 / 1024 / 1024),
                });
                break;

            case VmChart.Network:
                Title = L["Network"];
                Icon = PveBlazorHelper.Icons.Network;
                SeriesType = SeriesType.Area;
                ChartOptions = ChartOptionsKb;
                Name1 = L["In (KB)"];
                Name2 = L["Out (KB)"];
                Items = RrdData.Select(a => new Data()
                {
                    Date = a.TimeDate,
                    Value1 = Convert.ToDecimal(a.NetIn / 1024.0),
                    Value2 = Convert.ToDecimal(a.NetOut / 1024.0),
                });
                break;

            default: break;
        }
    }
}
