/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Node;
using DocumentFormat.OpenXml.Spreadsheet;
using Mapster;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components.ProxmoxVE.Nodes;

public partial class StorageUsage(IAdminService adminService) : IClusterName
{
    [EditorRequired, Parameter] public string ClusterName { get; set; } = default!;
    [EditorRequired, Parameter] public ClusterResource Resource { get; set; } = default!;

    private IEnumerable<NodeStorageContentEx> Contents { get; set; } = [];
    private IEnumerable<ClusterResource> Storages { get; set; } = [];
    private IEnumerable<VmDiskInfo> Disks { get; set; } = [];

    private class NodeStorageContentEx : NodeStorageContent, ISnapshotsSize, ISnapshotsReplicationSize
    {
        public double SnapshotsSize { get; set; }
        public double SnapshotsReplicationSize { get; set; }
    }

    private bool IsGroupByStorage => Resource.ResourceType == ClusterResourceType.Storage;
    private bool AllowCalculateSnapshotSize { get; set; }
    private bool IsLoading { get; set; }

    private IEnumerable<string> PropertiesName =>
    [
        IsGroupByStorage ? nameof(NodeStorageContent.VmId) : nameof(NodeStorageContent.Storage),
        nameof(NodeStorageContent.FileName),
        nameof(NodeStorageContent.Size),
        nameof(NodeStorageContent.CreationDate),
        nameof(NodeStorageContent.Format),
        nameof(NodeStorageContent.Content),
        nameof(NodeStorageContent.Verified),
        nameof(NodeStorageContent.Encrypted),
        nameof(ISnapshotsSize.SnapshotsSize),
        nameof(ISnapshotsReplicationSize.SnapshotsReplicationSize)
    ];

    private IEnumerable<GroupDescriptor> Groups
        => IsGroupByStorage
            ?
            [new ()
            {
                Title = L["Vm Id"],
                Property = nameof(NodeStorageContent.VmId)
            }]
            :
            [new()
            {
                Title = L["Storage"],
                Property = nameof(NodeStorageContent.Storage)
            }];

    private IEnumerable<string> Orderby
        => IsGroupByStorage
            ? [nameof(NodeStorageContent.VmId)]
            : [nameof(NodeStorageContent.Storage)];

    protected override async Task OnInitializedAsync()
    {
        IsLoading = true;

        var clusterClient = adminService[ClusterName];
        Storages = [.. (await clusterClient.CachedData.GetResourcesAsync(false)).Where(a => a.ResourceType == ClusterResourceType.Storage)];

        AllowCalculateSnapshotSize = clusterClient.Settings.AllowCalculateSnapshotSize;
        if (AllowCalculateSnapshotSize) { Disks = await clusterClient.CachedData.GetDisksInfoAsync(false); }

        Contents = (await GetContents())
                    .DistinctBy(a => a.Volume)
                    .OrderBy(a => a.Content)
                    .ThenBy(a => a.FileName);

        IsLoading = false;
    }

    public string GetInfo(ClusterResource info)
    {
        var ret = L["Used {0} of {1} {2} % - Free {3} - {4} %",
                 FormatHelper.FromBytes(info.DiskUsage),
                 FormatHelper.FromBytes(info.DiskSize),
                 Math.Round(100.0 / info.DiskSize * info.DiskUsage, 1),
                 FormatHelper.FromBytes(info.DiskSize - info.DiskUsage),
                 Math.Round(100.0 / info.DiskSize * (info.DiskSize - info.DiskUsage), 1)]
                 .ToString();

        if (AllowCalculateSnapshotSize)
        {
            ret += L[" - Snapshot {0} - For replication {1}",
                     FormatHelper.FromBytes(DiskInfoHelper.CalculateSnapshots(info.Node, info.Storage, Disks, false)),
                     FormatHelper.FromBytes(DiskInfoHelper.CalculateSnapshots(info.Node, info.Storage, Disks, true))];
        }

        return ret;
    }

    private async Task<IEnumerable<NodeStorageContentEx>> GetContents()
    {
        var ret = new List<NodeStorageContentEx>();
        var client = await adminService[ClusterName].GetPveClientAsync();

        void CalculateSnapshotsSize(IEnumerable<NodeStorageContentEx> items, string node)
        {
            var contentAllowed = new[] { "images", "rootdir" };
            foreach (var item in items.Where(a => contentAllowed.Contains(a.Content)))
            {
                item.SnapshotsSize = DiskInfoHelper.CalculateSnapshots(node, item.VmId, item.Storage, item.FileName, Disks, false);
                item.SnapshotsReplicationSize = DiskInfoHelper.CalculateSnapshots(node, item.VmId, item.Storage, item.FileName, Disks, true);
            }
        }

        switch (Resource.ResourceType)
        {
            case ClusterResourceType.All: break;
            case ClusterResourceType.Node: break;
            case ClusterResourceType.Vm:
                foreach (var node in (await client.GetNodesAsync()).Where(a => a.IsOnline))
                {
                    foreach (var storage in (await client.Nodes[node.Node].Storage.GetAsync(enabled: true))
                                            .Where(a => a.Active && a.Enabled))
                    {
                        var contents = (await client.Nodes[node.Node]
                                                    .Storage[storage.Storage]
                                                    .Content
                                                    .GetAsync(vmid: Convert.ToInt32(Resource.VmId)))
                                                    .AsQueryable()
                                                    .ProjectToType<NodeStorageContentEx>()
                                                    .ToList();

                        CalculateSnapshotsSize(contents, node.Node);

                        ret.AddRange(contents);
                    }
                }
                break;

            case ClusterResourceType.Storage:
                var storageContents = (await client.Nodes[Resource.Node].Storage[Resource.Storage].Content.GetAsync())
                                           .AsQueryable()
                                           .ProjectToType<NodeStorageContentEx>()
                                           .ToList();

                CalculateSnapshotsSize(storageContents, Resource.Node);

                ret.AddRange(storageContents);
                break;

            case ClusterResourceType.Pool: break;
            case ClusterResourceType.Sdn: break;
            case ClusterResourceType.Unknown: break;
            default: break;
        }

        return ret.Distinct()
                  .OrderBy(a => a.Storage)
                  .ThenBy(a => a.Content)
                  .ThenBy(a => a.VmId)
                  .ThenBy(a => a.Volume);
    }
}
