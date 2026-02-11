using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;

namespace Corsinvest.ProxmoxVE.Admin.Module.BackupAnalytics.Components;

public partial class Trends(IDbContextFactory<ModuleDbContext> dbContextFactory,
                            IAdminService adminService) : IClusterName
{
    [CascadingParameter(Name = nameof(ClusterName))] public string ClusterName { get; set; } = default!;

    private DateTime? Start { get; set; }
    private DateTime? End { get; set; }
    private DateTime? MinDate { get; set; }
    private DateTime? MaxDate { get; set; }
    private IList<StorageBackup> SelectedStorages { get; set; } = [];
    private IList<string> SelectedVmIds { get; set; } = [];
    private IEnumerable<string> VmIds { get; set; } = [];
    private IEnumerable<StorageBackup> Storages { get; set; } = [];
    private IEnumerable<Data> Items { get; set; } = [];

    private record StorageBackup(string Node, string Storage)
    {
        public string FullName => $"{Node} - {Storage}";
    };

    private record Data(string VmId,
                        string VmName,
                        string Descrition,
                        double Size,
                        double TransferSize,
                        DateTime Start,
                        DateTime End,
                        double TransferSpeed)
    {
        public double Duration => (End - Start).TotalSeconds;

        public DateTime Date => Start; //.Date;
    }

    protected override async Task OnInitializedAsync()
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        var query = db.TaskResults.FromClusterName(ClusterName);

        MinDate = await query.SelectMany(a => a.Jobs).Select(a => a.Start).MinAsync();
        MaxDate = await query.SelectMany(a => a.Jobs).Select(a => a.End).MaxAsync();
    }

    private DateTime StartUtc => DateTime.SpecifyKind(Start!.Value, DateTimeKind.Utc);
    private DateTime EndUtc => DateTime.SpecifyKind(End!.Value, DateTimeKind.Utc);

    private async Task LoadStoragesAsync()
    {
        SelectedStorages ??= [];

        if (End.HasValue)
        {
            await using var db = await dbContextFactory.CreateDbContextAsync();
            Storages = await db.TaskResults.FromClusterName(ClusterName)
                                           .Where(a => a.Start >= StartUtc && a.End <= EndUtc)
                                           .GroupBy(a => new { a.Node, a.Storage })
                                           .Select(a => new StorageBackup(a.Key.Node!, a.Key.Storage!))
                                           .ToListAsync();
        }
        else
        {
            Storages = [];
        }

        SelectedStorages = [.. SelectedStorages.Where(a => Storages.Contains(a))];

        await LoadVmIdsAsync();
    }

    private async Task LoadVmIdsAsync()
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();

        var nodes = SelectedStorages.Select(a => a.Node).ToList();
        var storages = SelectedStorages.Select(a => a.Storage).ToList();

        SelectedVmIds ??= [];
        VmIds = SelectedStorages.Any()
                    ? await db.JobResults.Where(a => a.TaskResult.ClusterName == ClusterName
                                                        && nodes.Contains(a.TaskResult.Node!)
                                                        && storages.Contains(a.TaskResult.Storage!)
                                                        && a.Start >= StartUtc && a.End <= EndUtc)
                                         .Select(a => a.VmId)
                                         .Distinct()
                                         .ToListAsync()
                    : [];

        SelectedVmIds = [.. SelectedVmIds.Where(a => VmIds.Contains(a))];

        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();

        var nodes = SelectedStorages.Select(a => a.Node).ToList();
        var storages = SelectedStorages.Select(a => a.Storage).ToList();

        var vms = SelectedVmIds.Any()
                    ? (await adminService[ClusterName].CachedData.GetResourcesAsync(false))
                        .Where(a => a.ResourceType == ClusterResourceType.Vm)
                        .Select(a => new
                        {
                            VmId = a.VmId.ToString(),
                            a.Name,
                            a.VmType,
                            a.Description
                        })
                        .ToList()
                    : [];

        Items = SelectedVmIds.Any()
                ? (await db.TaskResults.FromClusterName(ClusterName)
                                      .Where(a => nodes.Contains(a.Node!) && storages.Contains(a.Storage!)
                                                    && a.Start >= StartUtc && a.End <= EndUtc)
                                      .SelectMany(a => a.Jobs)
                                      .Where(a => SelectedVmIds.Contains(a.VmId) && a.End.HasValue)
                                      .OrderBy(a => a.Start)
                                      .Select(a => new
                                      {
                                          a.VmId,
                                          a.Size,
                                          a.TransferSize,
                                          Start = a.Start!.Value,
                                          End = a.End!.Value,
                                          a.TransferSpeed
                                      }).ToListAsync())
                                      .ConvertAll(a => new Data(a.VmId,
                                                                vms.Where(b => b.VmId == a.VmId).Select(a => a.Name).FirstOrDefault()!,
                                                                vms.Where(b => b.VmId == a.VmId).Select(a => a.Description).FirstOrDefault()!,
                                                                a.Size,
                                                                a.TransferSize,
                                                                a.Start,
                                                                a.End,
                                                                a.TransferSpeed))

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
