/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;

namespace Corsinvest.ProxmoxVE.Admin.Module.BackupAnalytics.Components;

public partial class UnprotectedDisks(IAdminService adminService) : IClusterName, IRefreshableData
{
    [CascadingParameter(Name = nameof(ClusterName))] public string ClusterName { get; set; } = default!;

    private IEnumerable<Data> Items { get; set; } = [];
    private bool IsLoading { get; set; }

    private class Data
    {
        public string Disks { get; set; } = default!;
        public string Type { get; set; } = default!;
        public string Node { get; set; } = default!;
        public string Description { get; set; } = default!;
        public string Status { get; set; } = default!;
        public bool IsLocked { get; set; } = default!;
        public long VmId { get; internal set; }
    }

    protected override async Task OnInitializedAsync() => await RefreshDataAsync();

    public async Task RefreshDataAsync()
    {
        IsLoading = true;

        var ret = new List<Data>();
        var clusterClient = adminService[ClusterName];
        var vms = (await clusterClient.CachedData.GetResourcesAsync(false))
                    .Where(a => a.ResourceType == ClusterResourceType.Vm)
                    .ToList();

        foreach (var item in vms)
        {
            var config = await clusterClient.CachedData.GetVmConfigAsync(item.Node, item.VmType, item.VmId, false);
            var disks = config.Disks.Where(a => !a.Backup);
            if (disks.Any())
            {
                ret.Add(new()
                {
                    Type = item.Type,
                    Status = item.Status,
                    IsLocked = item.IsLocked,
                    Node = item.Node,
                    VmId = item.VmId,
                    Description = item.Description,
                    Disks = disks.Select(a => $"{a.Storage}:{a.FileName}").JoinAsString("<br />")
                });
            }
        }

        IsLoading = false;
        Items = ret;
    }
}
