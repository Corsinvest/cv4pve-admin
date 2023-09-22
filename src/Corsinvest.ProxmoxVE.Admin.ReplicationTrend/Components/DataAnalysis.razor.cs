/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using ApexCharts;
using Corsinvest.AppHero.Core.MudBlazorUI.Style;
using Corsinvest.ProxmoxVE.Admin.Core.Services;
using Corsinvest.ProxmoxVE.Admin.ReplicationTrend.Repository;
using Corsinvest.ProxmoxVE.Api.Shared.Utils;

namespace Corsinvest.ProxmoxVE.Admin.ReplicationTrend.Components;

public partial class DataAnalysis
{
    [Inject] private IReadRepository<ReplicationResult> ReplicationResults { get; set; } = default!;
    [Inject] private IPveClientService PveClientService { get; set; } = default!;
    [Inject] private LayoutService LayoutService { get; set; } = default!;

    private ApexChartOptions<ReplicationResult> Options1 => new()
    {
        Markers = new()
        {
            Shape = ShapeEnum.Circle,
            Size = 5,
            FillOpacity = new Opacity(0.8d),
        },
        Chart = new()
        {
            Group = "ReplicationTrend",
            Background = "trasparent"
        },
        Yaxis = new() { new YAxis { DecimalsInFloat = 0 } },
        Theme = new() { Mode = LayoutService.IsDarkMode ? Mode.Dark : Mode.Light }
    };

    private ApexChartOptions<ReplicationResult> Options2 => new()
    {
        Markers = new()
        {
            Shape = ShapeEnum.Circle,
            Size = 5,
            FillOpacity = new Opacity(0.8d),
        },
        Chart = new()
        {
            Group = "ReplicationTrend",
            Background = "trasparent"
        },
        Yaxis = new() { new YAxis { DecimalsInFloat = 0 } },
        Theme = new() { Mode = LayoutService.IsDarkMode ? Mode.Dark : Mode.Light }
    };

    class Data
    {
        public string Title { get; set; } = default!;
        public int CountOk { get; set; }
        public int CountKo { get; set; }
        public string Size { get; set; } = default!;
        public DateRange DateRange { get; set; } = default!;
        public string VmId { get; set; } = default!;
    }

    private ApexChart<ReplicationResult> RefChart1 { get; set; } = default!;
    private ApexChart<ReplicationResult> RefChart2 { get; set; } = default!;
    private string? VmIdSelected { get; set; }
    private IEnumerable<Data> Dates { get; set; } = Array.Empty<Data>();
    private IEnumerable<Data> Vms { get; set; } = Array.Empty<Data>();
    private Data DateSelected { get; set; } = default!;
    private DateRange DateRange { get; set; } = new(DateTime.Now.AddDays(0).Date, DateTime.Now.Date);
    private IEnumerable<IGrouping<string?, ReplicationResult>> DataChart { get; set; } = Array.Empty<IGrouping<string?, ReplicationResult>>();
    private string _clusterName { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            _clusterName = await PveClientService.GetCurrentClusterNameAsync();
        }
        catch { }

        Dates = await GetDates();
        await OnClickDate(Dates.ToList()[0]);
    }

    private async Task<IEnumerable<Data>> GetDates()
    {
        var range = new List<(string Name, int Start, int End)>
        {
            ( L["Today"], 0, 1),
            ( L["Yesterday"], 1, 1),
            ( L["2 Day ago"], 2, 1),
            ( L["3 Day ago"], 3, 1),
            ( L["4 Day ago"], 4, 1),
            ( L["5 Day ago"], 5, 1),
            ( L["6 Day ago"], 6, 1),
            ( L["1 Week"], 7, 7),
            ( L["2 Week"], 14, 7),
            ( L["3 Week"], 21, 7),
            ( L["4 Week"], 28, 7),
            ( L["1 Month"], 30, 30),
            ( L["2 Month"], 60, 30),
            ( L["3 Month"], 60, 30),
        };

        var ret = new List<Data>();
        foreach (var (name, startR, endR) in range)
        {
            var start = DateTime.Now.Date.AddDays(startR * -1);
            var end = start.AddDays(endR);
            ret.Add(await Calculate(name, start, end));
        }

        return ret;
    }

    private async Task<Data> Calculate(string name, DateTime start, DateTime end)
        => new()
        {
            Title = name,
            CountOk = await ReplicationResults.CountAsync(new ReplicationResultSpec(_clusterName, start, end, true)),
            CountKo = await ReplicationResults.CountAsync(new ReplicationResultSpec(_clusterName, start, end, false)),
            Size = FormatHelper.FromBytes((await ReplicationResults.ListAsync(new ReplicationResultSpec(_clusterName, start, end, true))).Sum(a => a.Size)),
            DateRange = new DateRange(start, end)
        };

    private async Task DateRangeChanged(DateRange dateRange)
    {
        DateRange = dateRange;
        DateSelected = await Calculate($"{dateRange.Start.Value.ToShortDateString()} - {dateRange.End.Value.ToShortDateString()}",
                                       dateRange.Start.Value,
                                       dateRange.End.Value);
    }

    private async Task OnClickDate(Data item)
    {
        DateRange = item.DateRange;
        Vms = await GetVms();
        await OnClickVm(null);
    }

    private async Task<IEnumerable<Data>> GetVms()
        => (await GetData(true, true, false))
            .GroupBy(a => a.VmId)
            .Select(a => new Data
            {
                Title = a.Key!,
                CountOk = a.Count(a => a.Status),
                CountKo = a.Count(a => !a.Status),
                Size = FormatHelper.FromBytes(a.Where(a => a.Status).Sum(a => a.Size)),
                VmId = a.Key!,
            });

    private async Task OnClickVm(Data? item)
    {
        VmIdSelected = item?.VmId;
        DataChart = await GetDataChart();

        try
        {
            //FIX refresh chart
            if (RefChart1 != null)
            {
                await RefChart1.UpdateOptionsAsync(true, true, true);
                await RefChart1.UpdateOptionsAsync(true, true, true);
            }

            if (RefChart2 != null) { await RefChart2.UpdateOptionsAsync(true, true, true); }
        }
        catch { }

        StateHasChanged();
    }

    private async Task<IEnumerable<IGrouping<string?, ReplicationResult>>> GetDataChart()
        => (await GetData(true, true, true))
                .GroupBy(a => a.VmId);

    private async Task<IQueryable<ReplicationResult>> GetData(bool whereDate, bool whereStorage, bool whereVm)
        => (await ReplicationResults.ListAsync(new ReplicationResultSpec(_clusterName)
                                                    .InDate(whereDate, DateRange.Start, DateRange.End)
                                                    .InVm(whereVm && !string.IsNullOrWhiteSpace(VmIdSelected), VmIdSelected)))
                                                    .AsQueryable();
}