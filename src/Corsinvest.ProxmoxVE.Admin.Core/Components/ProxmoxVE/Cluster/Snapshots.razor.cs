/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Components.ProxmoxVE.Cluster;

public partial class Snapshots(IAdminService adminService) : IClusterName
{
    [EditorRequired, Parameter] public string ClusterName { get; set; } = default!;

    private bool IsLoading { get; set; }
    private IEnumerable<VmDiskInfo> _disks = [];

    private sealed record Data(string Type,
                               string Host,
                               string Space,
                               int Snapshots,
                               double Size,
                               int VMs);

    protected override async Task OnInitializedAsync() => await RefreshDataAsync();

    private async Task RefreshDataAsync()
    {
        IsLoading = true;
        _disks = await adminService[ClusterName].CachedData.GetDisksInfoAsync(false);
        IsLoading = false;
    }

    private List<Data> GetItems(bool replication)
    {
        var ret = new List<Data>();
        foreach (var item in _disks.GroupBy(a => new { a.Type, a.Host, a.SpaceName }))
        {
            var snaspshots = item.SelectMany(a => a.Snapshots).Where(a => a.Replication == replication);
            ret.Add(new(item.Key.Type,
                        item.Key.Host,
                        item.Key.SpaceName,
                        snaspshots.Count(),
                        snaspshots.Sum(a => a.Size),
                        item.DistinctBy(a => a.VmId).Count()));
        }
        return ret;
    }
}
