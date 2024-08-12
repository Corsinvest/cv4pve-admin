/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.BaseUI.DataManager;
using Corsinvest.AppHero.Core.Extensions;
using Corsinvest.ProxmoxVE.Admin.Core.Services;
using Corsinvest.ProxmoxVE.Api;
using Corsinvest.ProxmoxVE.Api.Extension;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;
using Corsinvest.ProxmoxVE.Api.Shared.Utils;
using System.ComponentModel.DataAnnotations;

namespace Corsinvest.ProxmoxVE.Admin.QemuMonitor;

public partial class RenderIndex
{
    [Inject] private IPveClientService PveClientService { get; set; } = default!;
    [Inject] private IDataGridManager<BlockStats> DataGridManager { get; set; } = default!;

    private readonly List<BlockStats> _data = [];

    class BlockStats
    {
        public long VmId { get; set; }
        public string Name { get; set; } = default!;
        public string Node { get; set; } = default!;
        public string Device { get; set; } = default!;
        public string Status { get; set; } = default!;
        public string Type { get; set; } = default!;
        public bool IsLocked { get; set; }

        [DisplayFormat(DataFormatString = "{0:" + FormatHelper.FormatBytes + "}")]
        public long ReadBytes { get; set; }

        [DisplayFormat(DataFormatString = "{0:" + FormatHelper.FormatBytes + "}")]
        public long WriteBytes { get; set; }

        public long ReadIops { get; set; }
        public long WriteIops { get; set; }

        [DisplayFormat(DataFormatString = "{0:" + FormatHelper.FormatBytes + "}")]
        public long ReadBytesSec { get; set; }

        [DisplayFormat(DataFormatString = "{0:" + FormatHelper.FormatBytes + "}")]
        public long WriteBytesSec { get; set; }
        public long ReadIopsSec { get; set; }
        public long WriteIopsSec { get; set; }
        public DateTime Timestamp { get; set; }
    }

    private static BlockStats Create(string data, long vmId, string vmName, string node, DateTime timestamp)
    {
        var values = data.Split(' ');

        long GetValue(string name)
            => long.Parse(values.Where(a => a.StartsWith($"{name}="))
                   .First()
                   .Split('=')[1]);

        return new()
        {
            Device = values[0], //[1..^1];
            ReadBytes = GetValue("rd_bytes"),
            WriteBytes = GetValue("wr_bytes"),
            ReadIops = GetValue("rd_operations"),
            WriteIops = GetValue("wr_operations"),
            VmId = vmId,
            Name = vmName,
            Node = node,
            Timestamp = timestamp
        };
    }

    protected override void OnInitialized()
    {
        DataGridManager.Title = L["QEMU Monitor"];
        DataGridManager.QueryAsync = async () => await LoadData();
    }

    private async Task<List<BlockStats>> LoadData()
    {
        var client = await PveClientService.GetClientCurrentClusterAsync();

        var timestamp = DateTime.Now;
        foreach (var vm in (await client.GetVmsAsync()).Where(a => a.IsRunning && a.VmType == VmType.Qemu))
        {
            var data = (await client.Nodes[vm.Node].Qemu[vm.VmId].Monitor.Monitor("info blockstats"))
                            .ToData<string>()
                            .SplitNewLine()
                            .Where(a => !string.IsNullOrWhiteSpace(a))
                            .Select(a => Create(a, vm.VmId, vm.Name, vm.Node, timestamp));

            foreach (var item in data)
            {
                var row = _data.Where(a => a.VmId == item.VmId && a.Device == item.Device).FirstOrDefault();
                if (row == null)
                {
                    item.Status = vm.Status;
                    item.IsLocked = vm.IsLocked;
                    item.Type = vm.Type;

                    _data.Add(item);
                }
                else
                {
                    row.Status = vm.Status;
                    row.IsLocked = vm.IsLocked;
                    row.Type = vm.Type;

                    //update with diff
                    row.ReadBytesSec = (item.ReadBytes - row.ReadBytes) / (long)(item.Timestamp - row.Timestamp).TotalSeconds;
                    row.WriteBytesSec = (item.WriteBytes - row.WriteBytes) / (long)(item.Timestamp - row.Timestamp).TotalSeconds;
                    row.ReadIopsSec = (item.ReadIops - row.ReadIops) / (long)(item.Timestamp - row.Timestamp).TotalSeconds;
                    row.WriteIopsSec = (item.WriteIops - row.WriteIops) / (long)(item.Timestamp - row.Timestamp).TotalSeconds;
                }
            }
        }

        return _data;
    }
}