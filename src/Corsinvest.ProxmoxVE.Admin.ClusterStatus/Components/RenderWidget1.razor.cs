/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Models;
using Corsinvest.ProxmoxVE.Api;
using Corsinvest.ProxmoxVE.Api.Extension;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Node;

namespace Corsinvest.ProxmoxVE.Admin.ClusterStatus.Components;

public partial class RenderWidget1
{
    [Inject] private IPveClientService PveClientService { get; set; } = default!;

    private IEnumerable<ResourceUsage> DataUsages { get; set; } = default!;
    private int Count { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var pveClient = await PveClientService.GetClientCurrentCluster();
        var resources = (await pveClient.GetResources(ClusterResourceType.All))
                            .CalculateHostUsage();
        DataUsages = ResourceUsage.GetUsages(resources, L);

        //to fix with new release extensions
        Count = (await pveClient.Cluster.Tasks.Tasks())
                .ToModel<IEnumerable<NodeTask>>()
                .Count(a => a.StatusOk);

        //await CreateGenealInfo();
    }

    //private enum TypeData
    //{
    //    Licenses,
    //    Updates,
    //    AllTasks,
    //    Backups,
    //    StoragesUsage,
    //    VmsReplications,
    //    OnLine,
    //    NodesEndOfLine
    //}

    //private record class Info(bool Valid, string Text, TypeData Type, List<string> Errors);

    //private async Task ExpandedChanged(bool isExpanded, Info info)
    //{
    //    if (isExpanded && !info.Errors.Any())
    //    {
    //        foreach (var node in await _pveClient.GetNodes())
    //        {
    //            var pveNode = _pveClient.Nodes[node.Node];

    //            switch (info.Type)
    //            {
    //                case TypeData.Licenses:
    //                    if (await CheckLicense(pveNode)) { info.Errors.Add(node.Node); }
    //                    break;

    //                case TypeData.Updates:
    //                    var updates = (await pveNode.Apt.Update.Get()).GroupBy(a => a.Priority).OrderBy(a => a.Key);
    //                    if (updates.Any())
    //                    {
    //                        info.Errors.Add($"Node {node.Node}:");
    //                        foreach (var item in updates)
    //                        {
    //                            info.Errors.Add($"{item.Key} {item.Count()}");
    //                        }
    //                    }
    //                    break;

    //                case TypeData.AllTasks:
    //                    var dateCheck = new DateTimeOffset(DateTime.Now.Date.AddDays(Options.Value.DaysPrevious)).ToUnixTimeSeconds();
    //                    foreach (var item in (await pveNode.Tasks.Get(errors: true, limit: 100))
    //                                            .Where(a => a.StartTime >= dateCheck).
    //                                            GroupBy(a => a.Type))
    //                    {
    //                        info.Errors.Add($"Node {node.Node} error tasks {item.Key} {item.Count()}");
    //                    }
    //                    break;

    //                case TypeData.Backups:
    //                    break;

    //                case TypeData.StoragesUsage:
    //                    foreach (var item in (await pveNode.Storage.Get()).Where(a => a.Active && a.UsagePercentage * 100.0 > Options.Value.MaxStorageUsage))
    //                    {
    //                        info.Errors.Add($"Node {node.Node} storage {item.Storage} usage {item.UsagePercentage * 100.0}% > {Options.Value.MaxStorageUsage}%");
    //                    }
    //                    break;

    //                case TypeData.VmsReplications:
    //                    foreach (var item in await pveNode.Replication.Get())
    //                    {
    //                        if (item.ExtensionData != null && item.ExtensionData.ContainsKey("error"))
    //                        {
    //                            info.Errors.Add($"Node {node.Node} vm {item.Id} replication error");
    //                        }
    //                    }
    //                    break;

    //                case TypeData.Online:
    //                    if (!node.IsOnLine) { info.Errors.Add(node.Node); }
    //                    break;

    //                case TypeData.NodesEndOfLine:
    //                    if (await CheckEOL(pveNode)) { info.Errors.Add(node.Node); }
    //                    break;

    //                default: break;
    //            }
    //        }
    //    }
    //}

    ///// <summary>
    ///// Create General Info
    ///// </summary>
    ///// <returns></returns>
    //public async Task CreateGenealInfo()
    //{
    //    var licenses = true;
    //    var updates = true;
    //    var allTasks = true;
    //    var tasksBackupInErrors = true;
    //    var storagesUsage = true;
    //    var replications = true;
    //    var useReplications = true;
    //    var online = true;
    //    var nodesEndOfLine = true;

    //    foreach (var node in await _pveClient.GetNodes())
    //    {
    //        if (!node.IsOnLine)
    //        {
    //            online = false;
    //            continue;
    //        }

    //        var pveNode = _pveClient.Nodes[node.Node];

    //        if (licenses && await CheckLicense(pveNode)) { licenses = false; }
    //        if (updates && (await pveNode.Apt.Update.Get()).Any()) { updates = false; }

    //        if (allTasks)
    //        {
    //            var dateCheck = new DateTimeOffset(DateTime.Now.Date.AddDays(Options.Value.DaysPrevious)).ToUnixTimeSeconds();
    //            var exists = (await pveNode.Tasks.Get(errors: true, limit: 1)).Any(a => a.StartTime >= dateCheck);
    //            if (exists) { allTasks = false; }
    //        }

    //        if (tasksBackupInErrors)
    //        {
    //            var dateCheck = new DateTimeOffset(DateTime.Now.Date.AddDays(Options.Value.DaysPrevious)).ToUnixTimeSeconds();
    //            var exists = (await pveNode.Tasks.Get(errors: true, limit: 1, typefilter: "vzdump")).Any(a => a.StartTime >= dateCheck);
    //            if (exists) { tasksBackupInErrors = false; }
    //        }

    //        if (storagesUsage)
    //        {
    //            var exists = (await pveNode.Storage.Get()).Any(a => a.Active && a.UsagePercentage * 100.0 > Options.Value.MaxStorageUsage);
    //            if (exists) { storagesUsage = false; }
    //        }

    //        var replicationsData = await pveNode.Replication.Get();
    //        if (replications && replicationsData.Any())
    //        {
    //            useReplications = true;
    //            foreach (var item in replicationsData)
    //            {
    //                if (item.ExtensionData != null && item.ExtensionData.ContainsKey("error"))
    //                {
    //                    replications = false;
    //                    break;
    //                }
    //            }
    //        }

    //        if (nodesEndOfLine && await CheckEOL(pveNode)) { nodesEndOfLine = false; }
    //    }

    //    GeneralInfo.Add(new(online, "Nodes On line", TypeData.Online, new()));
    //    GeneralInfo.Add(new(licenses, "Nodes License", TypeData.Licenses, new()));
    //    GeneralInfo.Add(new(updates, "Nodes Update", TypeData.Updates, new()));
    //    GeneralInfo.Add(new(storagesUsage, "Usage Storages", TypeData.StoragesUsage, new()));
    //    GeneralInfo.Add(new(allTasks, "All Tasks", TypeData.AllTasks, new()));
    //    GeneralInfo.Add(new(tasksBackupInErrors, "Tasks Backup", TypeData.Backups, new()));

    //    if (useReplications) { GeneralInfo.Add(new(replications, "VMs Replication", TypeData.VmsReplications, new())); }
    //    if (!nodesEndOfLine) { GeneralInfo.Add(new(nodesEndOfLine, "Nodes End Of Life", TypeData.NodesEndOfLine, new())); }
    //}

    //private static async Task<bool> CheckLicense(PveClient.PveNodes.PveNodeItem node)
    //    => (await node.Subscription.GetEx()).Status != "Active";

    //private static async Task<bool> CheckEOL(PveClient.PveNodes.PveNodeItem node)
    //{
    //    var endOfLife = new Dictionary<string, DateTime>()
    //            {
    //                {"6" , new DateTime(2022, 07, 01)},
    //                {"5" , new DateTime(2020, 07, 01)},
    //                {"4" , new DateTime(2018, 06, 01)},
    //            };

    //    return endOfLife.TryGetValue((await node.Version.Get()).Version.Split('.')[0], out _);
    //}
}