/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */

using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;

namespace Corsinvest.ProxmoxVE.Admin.Module.Resources.Components;

public partial class Disks(IAdminService adminService) : IClusterName, IRefreshableData
{
    [CascadingParameter(Name = nameof(ClusterName))] public string ClusterName { get; set; } = default!;

    private RadzenDataGrid<Data> DataGridRef { get; set; } = default!;
    private List<Data> Items { get; set; } = [];
    private bool IsLoading { get; set; }

    private record Data(string Node,
                        long VmId,
                        string Name,
                        string Guest,
                        string Type,
                        string Status,
                        bool IsLocked,
                        VmDiskKind Kind,
                        string Id,
                        string Storage,
                        string? PluginType,
                        bool Shared,
                        string FileName,
                        double Size,
                        double? UsagePct,
                        string Cache,
                        bool Backup,
                        bool IsUnused,
                        string Device,
                        string MountPoint,
                        string MountSourcePath,
                        bool Passthrough,
                        string Prealloc,
                        string Format);

    protected override Task OnInitializedAsync() => RefreshDataAsync();

    public async Task RefreshDataAsync()
    {
        IsLoading = true;
        Items = [];

        await InvokeAsync(StateHasChanged);

        var clusterClient = adminService[ClusterName];

        var resources = await clusterClient.CachedData.GetResourcesAsync(false);

        var storageLookup = resources.Where(a => a.ResourceType == ClusterResourceType.Storage)
                                     .ToDictionary(a => (a.Node, a.Storage));

        Items = [.. (await ParallelHelper.RunManyAsync(
                        resources.Where(a => a.ResourceType == ClusterResourceType.Vm),
                        async item =>
                        {
                            var config = await clusterClient.CachedData.GetGuestConfigAsync(item.Node, item.VmType, item.VmId, false);
                            return config.DisksAll.Select(disk =>
                            {
                                storageLookup.TryGetValue((item.Node, disk.Storage), out var storageRes);

                                return new Data(item.Node,
                                                item.VmId,
                                                item.Name,
                                                item.Description,
                                                item.Type,
                                                item.Status,
                                                item.IsLocked,
                                                disk.Kind,
                                                disk.Id,
                                                disk.Storage,
                                                storageRes?.PluginType,
                                                storageRes?.Shared ?? false,
                                                disk.FileName,
                                                disk.SizeBytes,

                                                storageRes is { DiskSize: > 0 }
                                                    ? (double)disk.SizeBytes / storageRes.DiskSize
                                                    : null,

                                                disk.Cache,
                                                disk.Backup,
                                                disk.IsUnused,
                                                disk.Device,
                                                disk.MountPoint,
                                                disk.MountSourcePath,
                                                disk.Passthrough,
                                                disk.Prealloc,
                                                disk.Format);
                            });
                        }))
                        .OrderBy(a => a.Node)
                        .ThenBy(a => a.Type)
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
