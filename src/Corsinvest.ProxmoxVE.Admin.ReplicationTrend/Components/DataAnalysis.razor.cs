/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */

using ApexCharts;
using Corsinvest.ProxmoxVE.Admin.Core.Services;
using Corsinvest.ProxmoxVE.Admin.ReplicationTrend.Repository;
using Corsinvest.ProxmoxVE.Api.Shared.Utils;

namespace Corsinvest.ProxmoxVE.Admin.ReplicationTrend.Components;

public partial class DataAnalysis
{
    [Inject] private IReadRepository<ReplicationResult> ReplicationResults { get; set; } = default!;
    [Inject] private IPveClientService PveClientService { get; set; } = default!;

    private static ApexChartOptions<ReplicationResult> Options1 => new()
    {
        Markers = new()
        {
            Shape = ShapeEnum.Circle,
            Size = 5,
            FillOpacity = new Opacity(0.8d),
        },
        Yaxis = new() { new YAxis { DecimalsInFloat = 0 } }
    };

    private static ApexChartOptions<ReplicationResult> Options2 => new()
    {
        Markers = new()
        {
            Shape = ShapeEnum.Circle,
            Size = 5,
            FillOpacity = new Opacity(0.8d),
        },
        Yaxis = new() { new YAxis { DecimalsInFloat = 0 } }
    };

    class Data<T>
    {
        public string Title { get; set; } = default!;
        public int CountOk { get; set; }
        public int CountKo { get; set; }
        public string Size { get; set; } = default!;
        public T Tag { get; set; } = default!;
        public T Tag1 { get; set; } = default!;
    }

    private ApexChart<ReplicationResult> RefChart1 { get; set; } = default!;
    private ApexChart<ReplicationResult> RefChart2 { get; set; } = default!;
    private DateTime? DateSelectedStart { get; set; }
    private DateTime? DateSelectedEnd { get; set; }
    private string? DateSelectedTitle { get; set; }
    private string? VmIdSelected { get; set; }
    private IEnumerable<Data<DateTime>> Dates { get; set; } = Array.Empty<Data<DateTime>>();
    private IEnumerable<Data<string>> Vms { get; set; } = Array.Empty<Data<string>>();
    private string ClusterName { get; set; } = default!;

    private IEnumerable<IGrouping<string?, ReplicationResult>> DataChart { get; set; } = Array.Empty<IGrouping<string?, ReplicationResult>>();

    protected override async Task OnInitializedAsync()
    {
        try
        {
            ClusterName = await PveClientService.GetCurrentClusterName();
        }
        catch         {        }

        Dates = await GetDates();
        await OnClickDate(Dates.ToList()[0]);
    }

    private async Task<IEnumerable<Data<DateTime>>> GetDates()
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

        var ret = new List<Data<DateTime>>();
        foreach (var (name, startR, endR) in range)
        {
            var start = DateTime.Now.Date.AddDays(startR * -1);
            var end = start.AddDays(endR);
            ret.Add(new()
            {
                Title = name,
                CountOk = await ReplicationResults.CountAsync(new ReplicationResultSpec(ClusterName, start, end, true)),
                CountKo = await ReplicationResults.CountAsync(new ReplicationResultSpec(ClusterName, start, end, false)),
                Size = FormatHelper.FromBytes((await ReplicationResults.ListAsync(new ReplicationResultSpec(ClusterName, start, end, true))).Sum(a => a.Size)),
                Tag = start,
                Tag1 = end
            });
        }

        return ret;
    }

    private async Task OnClickDate(Data<DateTime> item)
    {
        DateSelectedStart = item.Tag;
        DateSelectedEnd = item.Tag1;
        DateSelectedTitle = item.Title;
        Vms = await GetVms();
        await OnClickVm(null);
    }

    private async Task<IEnumerable<Data<string>>> GetVms()
        => (await GetData(true, true, false))
            .GroupBy(a => a.VmId)
            .Select(a => new Data<string>
            {
                Title = a.Key!,
                CountOk = a.Count(a => a.Status),
                CountKo = a.Count(a => !a.Status),
                Size = FormatHelper.FromBytes(a.Where(a => a.Status).Sum(a => a.Size)),
                Tag = a.Key!,
            });

    private async Task OnClickVm(Data<string>? item)
    {
        VmIdSelected = item?.Tag;
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
        => (await ReplicationResults.ListAsync(new ReplicationResultSpec(ClusterName)
                                                    .InDate(whereDate && DateSelectedStart.HasValue, DateSelectedStart, DateSelectedEnd)
                                                    .InVm(whereVm && !string.IsNullOrWhiteSpace(VmIdSelected), VmIdSelected)))
                                                    .AsQueryable();
}