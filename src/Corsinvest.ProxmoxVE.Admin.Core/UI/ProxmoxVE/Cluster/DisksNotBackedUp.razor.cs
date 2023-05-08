/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.Security.Auth.Permissions;
using Corsinvest.ProxmoxVE.Api;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;
using Mapster;
using System.ComponentModel;

namespace Corsinvest.ProxmoxVE.Admin.Core.UI.ProxmoxVE.Cluster;

public partial class DisksNotBackedUp
{
    [Parameter] public string Height { get; set; } = default!;
    [EditorRequired][Parameter] public PveClient PveClient { get; set; } = default!;
    [Parameter] public PermissionsRead Permissions { get; set; } = default!;

    [Inject] private IDataGridManager<Data> DataGridManager { get; set; } = default!;


    class Data : ClusterResource
    {
        [Display(Name = "Disk")]
        public string Disks { get; set; } = default!;
    }

    protected override void OnInitialized()
    {
        DataGridManager.Title = L["Disks not backed up"];
        DataGridManager.DefaultSort = new()
        {
            [nameof(Data.Type)] = false,
            [nameof(Data.Name)] = false,
        };
        DataGridManager.QueryAsync = GetDisksNotBackupped;
    }

    private async Task<IEnumerable<Data>> GetDisksNotBackupped()
    {
        var ret = new List<Data>();
        var vms = (await PveClient.GetVms())
                    .AsQueryable()
                    .ProjectToType<Data>()
                    .ToArray();

        foreach (var item in vms)
        {
            VmConfig config = item.VmType switch
            {
                VmType.Qemu => await PveClient.Nodes[item.Node].Qemu[item.VmId].Config.Get(),
                VmType.Lxc => await PveClient.Nodes[item.Node].Lxc[item.VmId].Config.Get(),
                _ => throw new InvalidEnumArgumentException(),
            };

            var disks = config.Disks.Where(a => !a.Backup);

            if (disks.Any())
            {
                item.Disks = disks.Select(a => $"{a.Storage}:{a.FileName}").JoinAsString(",");
                ret.Add(item);
            }
        }

        return ret;
    }
}