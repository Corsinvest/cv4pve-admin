/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.ClusterUsage.Repository;
using Corsinvest.ProxmoxVE.Admin.Core.Extensions;
using Corsinvest.ProxmoxVE.Api.Extension;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Common;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;
using Microsoft.Extensions.Logging;
using System.ComponentModel;

namespace Corsinvest.ProxmoxVE.Admin.ClusterUsage;

internal class Helper
{
    public static async Task Scan(IServiceScope scope, string clusterName)
    {
        var loggerFactory = scope.GetLoggerFactory();
        var logger = loggerFactory.CreateLogger(typeof(Helper));

        using (logger.LogTimeOperation(LogLevel.Information, true, "Collect usage"))
        {
            var client = await scope.GetPveClient(clusterName);
            //var db = scope.ServiceProvider.GetRequiredService<ClusterUsageDbContext>();
            var dataVms = scope.GetRepository<DataVm>();

            var date = DateTime.Now.AddDays(-1).Date;
            var end = date.AddDays(1).Date;

            foreach (var vm in await client.GetVms())
            {
                if (!(await dataVms.AnyAsync(new DataVmSpec(clusterName, vm.VmId, date))))
                {
                    var pveNode = client.Nodes[vm.Node];
                    var rrdData = (vm.VmType switch
                    {
                        VmType.Qemu => await pveNode.Qemu[vm.VmId].Rrddata.Get(RrdDataTimeFrame.Day, RrdDataConsolidation.Average),
                        VmType.Lxc => await pveNode.Lxc[vm.VmId].Rrddata.Get(RrdDataTimeFrame.Day, RrdDataConsolidation.Average),
                        _ => throw new InvalidEnumArgumentException(),
                    })
                    .Where(a => a.TimeDate >= date && a.TimeDate <= end);

                    var data = new DataVm
                    {
                        ClusterName = clusterName,
                        Date = date,
                        VmId = vm.VmId,
                        VmName = vm.Name,
                        Node = vm.Node,
                        CpuSize = Convert.ToInt32(vm.CpuSize),
                        CpuUsagePercentage = rrdData.Select(a => a.CpuUsagePercentage).DefaultIfEmpty(0).Average(),
                        MemorySize = vm.MemorySize,
                        MemoryUsage = Convert.ToInt64(rrdData.Select(a => a.MemoryUsage).DefaultIfEmpty(0).Average())
                    };

                    //storages
                    foreach (var storage in (await pveNode.Storage.Get(enabled: true)).Where(a => a.Active && a.Enabled))
                    {
                        var content = await pveNode.Storage[storage.Storage].Content.Get(vmid: Convert.ToInt32(vm.VmId));
                        if (content.Any())
                        {
                            data.Storages.Add(new()
                            {
                                Storage = storage.Storage,
                                Size = content.Sum(a => a.Size)
                            });
                        }
                    }

                    await dataVms.AddAsync(data);
                }
            }

            await dataVms.SaveChangesAsync();
        }
    }
}