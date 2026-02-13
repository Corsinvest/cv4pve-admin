/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.ComponentModel;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;

namespace Corsinvest.ProxmoxVE.Admin.Module.BackupAnalytics.Components;

public partial class UnprotectedDisks(IAdminService adminService) : IClusterName
{
    [CascadingParameter(Name = nameof(ClusterName))] public string ClusterName { get; set; } = default!;

    private IEnumerable<Data> Items { get; set; } = [];
    private bool IsLoading { get; set; }
    private IEnumerable<string> PropertiesName { get; } =
    [
        nameof(IClusterResourceVm.Status),
        nameof(IClusterResourceVm.Type),
        nameof(IClusterResourceVm.Node),
        nameof(IClusterResourceVm.Description)
        //nameof(IClusterResourceVm.DiskUsagePercentage),
        //nameof(IClusterResourceVm.MemoryUsagePercentage),
        //nameof(IClusterResourceVm.CpuUsagePercentage),
        //nameof(IClusterResourceVm.Uptime),
    ];

    private class Data : ClusterResource
    {
        public string Disks { get; set; } = default!;
    }

    protected override async Task OnInitializedAsync() => Items = await GetDisksNotBackupped();

    private async Task<IEnumerable<Data>> GetDisksNotBackupped()
    {
        IsLoading = true;
        var client = await adminService[ClusterName].GetPveClientAsync();

        var ret = new List<Data>();

        foreach (var item in await client.GetVmsAsync())
        {
            VmConfig config = item.VmType switch
            {
                VmType.Qemu => await client.Nodes[item.Node].Qemu[item.VmId].Config.GetAsync(),
                VmType.Lxc => await client.Nodes[item.Node].Lxc[item.VmId].Config.GetAsync(),
                _ => throw new InvalidEnumArgumentException()
            };

            var disks = config.Disks.Where(a => !a.Backup);
            if (disks.Any())
            {
                ret.Add(new Data
                {
                    Type = item.Type,
                    Node = item.Node,
                    Description = item.Description,
                    DiskUsagePercentage = item.DiskUsagePercentage,
                    MemoryUsagePercentage = item.MemoryUsagePercentage,
                    CpuUsagePercentage = item.CpuUsagePercentage,
                    Uptime = item.Uptime,
                    Disks = disks.Select(a => $"{a.Storage}:{a.FileName}").JoinAsString("<br />")
                });
            }
        }

        IsLoading = false;
        return ret;
    }
}
