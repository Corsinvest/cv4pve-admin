/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */

using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;

namespace Corsinvest.ProxmoxVE.Admin.Module.Resources.Components;

public partial class Partitions(IAdminService adminService) : IClusterName, IRefreshableData
{
    [CascadingParameter(Name = nameof(ClusterName))] public string ClusterName { get; set; } = default!;

    private RadzenDataGrid<Data> DataGridRef { get; set; } = default!;
    private List<Data> Items { get; set; } = [];
    private bool IsLoading { get; set; }

    private record Data(string Node,
                        long VmId,
                        string Name,
                        string Guest,
                        string Status,
                        bool IsLocked,
                        string MountPoint,
                        string FsName,
                        string Type,
                        ulong TotalBytes,
                        ulong UsedBytes,
                        double? UsedPct,
                        string? Error,
                        string Disks);

    protected override Task OnInitializedAsync() => RefreshDataAsync();

    public async Task RefreshDataAsync()
    {
        IsLoading = true;
        Items = [];

        await InvokeAsync(StateHasChanged);

        var clusterClient = adminService[ClusterName];

        var vms = (await clusterClient.CachedData.GetResourcesAsync(false))
                        .Where(a => a.ResourceType == ClusterResourceType.Vm
                                    && a.VmType == VmType.Qemu
                                    && a.IsRunning)
                        .OrderBy(a => a.Node)
                        .ThenBy(a => a.VmId);

        Items = [.. (await ParallelHelper.RunManyAsync(vms, async item =>
                    {
                        var partitions = await clusterClient.CachedData.GetQemuFsInfoAsync(item.Node, item.VmId, false);
                        return partitions.Select(p => new Data(item.Node,
                                                               item.VmId,
                                                               item.Name,
                                                               item.Description,
                                                               item.Status,
                                                               item.IsLocked,
                                                               p.MountPoint,
                                                               p.Name,
                                                               p.Type,
                                                               p.TotalBytes,
                                                               p.UsedBytes,
                                                               p.TotalBytes > 0 ? (double)p.UsedBytes / p.TotalBytes : null,
                                                               p.Error?.ToString(),
                                                               p.Disks.Select(d => $"{d.Dev} ({d.BusType})").JoinAsString(", ")));
                    }))
                    .OrderBy(a => a.Node)
                    .ThenBy(a => a.VmId)];

        IsLoading = false;
        await InvokeAsync(StateHasChanged);
    }

    private void Render(DataGridRenderEventArgs<Data> args)
    {
        if (args.FirstRender)
        {
            DataGridRef.Groups.Add(new()
            {
                Title = L["Guest"],
                Property = nameof(Data.Guest)
            });
            StateHasChanged();
        }
    }
}
