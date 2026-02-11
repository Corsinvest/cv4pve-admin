namespace Corsinvest.ProxmoxVE.Admin.Module.ReplicationAnalytics.Components;

public partial class Trends(IDbContextFactory<ModuleDbContext> dbContextFactory) : IClusterName
{
    [CascadingParameter(Name = nameof(ClusterName))] public string ClusterName { get; set; } = default!;

    private DateTime? Start { get; set; }
    private DateTime? End { get; set; }
    private DateTime? MinDate { get; set; }
    private DateTime? MaxDate { get; set; }
    private string SelectedSource { get; set; } = default!;
    private IList<string> SelectedVmIds { get; set; } = [];
    private IEnumerable<string> VmIds { get; set; } = [];
    private IEnumerable<string> Sources { get; set; } = [];
    private IEnumerable<Data> Items { get; set; } = [];

    private record Data(string VmId,
                        double Size,
                        DateTime Start,
                        DateTime End)
    {
        public double Duration => (End - Start).TotalSeconds;
    }

    protected override async Task OnInitializedAsync()
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        var query = db.JobResults.FromClusterName(ClusterName);

        if (await query.AnyAsync())
        {
            MinDate = await query.MinAsync(a => a.Start);
            MaxDate = await query.MaxAsync(a => a.End);
        }
    }
    private DateTime StartUtc => DateTime.SpecifyKind(Start!.Value, DateTimeKind.Utc);
    private DateTime EndUtc => DateTime.SpecifyKind(End!.Value, DateTimeKind.Utc);

    private async Task LoadSourcesAsync()
    {
        if (End.HasValue)
        {
            await using var db = await dbContextFactory.CreateDbContextAsync();
            Sources = await db.JobResults.FromClusterName(ClusterName)
                                         .Where(a => a.Start >= StartUtc && a.End <= EndUtc)
                                         .Select(a => a.Source)
                                         .Distinct()
                                         .ToListAsync();
        }
        else
        {
            Sources = [];
        }

        SelectedSource = Sources.Contains(SelectedSource)
                            ? SelectedSource
                            : string.Empty;

        await LoadVmIdsAsync();
    }

    private async Task LoadVmIdsAsync()
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        SelectedVmIds ??= [];

        VmIds = !string.IsNullOrEmpty(SelectedSource)
                    ? await db.JobResults.FromClusterName(ClusterName)
                                         .Where(a => a.Start >= StartUtc && a.End <= EndUtc
                                                        && a.Source == SelectedSource)
                                         .Select(a => a.VmId)
                                         .Distinct()
                                         .ToListAsync()
                    : [];

        SelectedVmIds = [.. SelectedVmIds.Where(a => VmIds.Contains(a))];
    }

    private async Task LoadDataAsync()
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();

        Items = SelectedVmIds.Any()
                    ? await db.JobResults.FromClusterName(ClusterName)
                                         .Where(a => a.Start >= StartUtc && a.End <= EndUtc
                                                        && a.Source == SelectedSource
                                                        && SelectedVmIds.Contains(a.VmId))
                                         .Select(a => new Data(a.VmId,
                                                               a.Size,
                                                               a.Start,
                                                               a.End!.Value))
                                         .ToListAsync()
                    : [];
    }

    //private static void DateRenderStart(DateRenderEventArgs args)
    //{
    //    //var date = args.Date.Date;
    //    //args.Disabled = date < MinDate!.Value.Date
    //    //                || !Results//.SelectMany(a => a.Jobs)
    //    //                           .Any(a => date.Day == a.Start.Day
    //    //                                     && date.Month == a.Start.Month
    //    //                                     && date.Year == a.Start.Year);
    //}

    //private static void DateRenderEnd(DateRenderEventArgs args)
    //{
    //    //if (!SelectedStartDate.HasValue) { return; }

    //    //var date = args.Date.Date;
    //    //args.Disabled = !SelectedStartDate.HasValue
    //    //                || (SelectedStartDate.HasValue && date < SelectedStartDate.Value)
    //    //                || !Results//.SelectMany(a => a.Jobs)
    //    //                           .Any(a => a.End.HasValue
    //    //                                    && date.Day == a.End.Value.Day
    //    //                                    && date.Month == a.End.Value.Month
    //    //                                    && date.Year == a.End.Value.Year);
    //}
}
