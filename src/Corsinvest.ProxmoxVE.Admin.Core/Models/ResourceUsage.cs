using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;

namespace Corsinvest.ProxmoxVE.Admin.Core.Models;

public class ResourceUsage
{
    public string Name { get; set; } = default!;
    public double Usage { get; set; }
    public string Info { get; set; } = default!;
    public string? Group { get; set; }
    public string Color => PveAdminUIHelper.GetColorRange(Usage);

    public static IList<ResourceUsage> Get(IEnumerable<ClusterResource> resources, IStringLocalizer L)
    {
        var nodes = resources.Where(a => a.ResourceType == ClusterResourceType.Node && a.IsOnline).ToList();

        //all storage not shared
        var allStorage = resources.Where(a => a.ResourceType == ClusterResourceType.Storage && a.IsAvailable);

        var storages = allStorage.Where(a => !a.Shared).ToList();
        storages.AddRange(allStorage.Where(a => a.Shared).DistinctBy(a => a.Storage));

        var memoryUsage = nodes.Sum(a => a.MemoryUsage);
        var memorySize = nodes.Sum(a => a.MemorySize);
        var diskUsage = storages.Sum(a => a.DiskUsage);
        var diskSize = storages.Sum(a => a.DiskSize);

        return
        [
            new()
            {
                Name = L["CPU"],
                Group="CPU",
                Usage = Math.Round(nodes.Average(a => a.CpuUsagePercentage) * 100, 1),
                Info = L["of {0} CPU(s)", nodes.Sum(a => a.CpuSize)]
            },
            new()
            {
                Name = L["Memory"],
                Group="Memory",
                Usage = Math.Round(Convert.ToDouble(memoryUsage) / memorySize * 100, 1),
                Info = L["{0} of {1}", FormatHelper.FromBytes(memoryUsage), FormatHelper.FromBytes(memorySize)]
            },
            new()
            {
                Name = L["Storage"],
                Group="Storage",
                Usage = Math.Round(Convert.ToDouble(diskUsage) / diskSize * 100, 1),
                Info = L["{0} of {1}", FormatHelper.FromBytes(diskUsage), FormatHelper.FromBytes(diskSize)]
            }
        ];
    }

    public static async Task<ResourceUsage> GetSnapshots(IEnumerable<ClusterResource> resources,
                                                          IStringLocalizer L,
                                                          ClusterClient clusterClient)
    {
        if (clusterClient.Settings.AllowCalculateSnapshotSize)
        {
            var disks = await clusterClient.CachedData.GetDisksInfoAsync(false);
            var snapshotSize = disks.SelectMany(a => a.Snapshots).Sum(a => a.Size);

            var allStorage = resources.Where(a => a.ResourceType == ClusterResourceType.Storage && a.IsAvailable);
            var storages = allStorage.Where(a => !a.Shared).ToList();
            storages.AddRange(allStorage.Where(a => a.Shared).DistinctBy(a => a.Storage));

            return new()
            {
                Name = L["Snapshot"],
                Group = "Snapshot",
                Usage = Math.Round(snapshotSize / storages.Sum(a => a.DiskSize) * 100, 1),
                Info = FormatHelper.FromBytes(snapshotSize)
            };
        }
        else
        {
            return new()
            {
                Name = L["Snapshot"],
                Group = "Snapshot",
                Usage = 0,
                Info = L["Not allowed"]
            };
        }
    }
}
