/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.Service;
using Corsinvest.ProxmoxVE.Admin.Diagnostic.Repository;
using Corsinvest.ProxmoxVE.Api.Extension.Utils;
using Corsinvest.ProxmoxVE.Diagnostic.Api;
using Mapster;
using Newtonsoft.Json;

namespace Corsinvest.ProxmoxVE.Admin.Diagnostic.Components;

public partial class Detail
{
    [Parameter] public int Id { get; set; }

    [Inject] private IRepository<IgnoredIssue> IgnoredIssuesRepo { get; set; } = default!;
    [Inject] private IReadRepository<Execution> ExecutionsRepo { get; set; } = default!;
    [Inject] private IOptionsSnapshot<Options> Options { get; set; } = default!;
    [Inject] private IDataGridManager<Data> DataGridManager { get; set; } = default!;
    [Inject] private IJobService JobService { get; set; } = default!;
    [Inject] private IBrowserService BrowserService { get; set; } = default!;

    private class Data : DiagnosticResult
    {
        public string? Url { get; set; }
    }

    private string _clusterName = default!;

    protected override void OnInitialized()
    {
        DataGridManager.Title = L["Results"];
        DataGridManager.DefaultSort = new()
        {
            [nameof(DiagnosticResult.Gravity)] = false,
            [nameof(DiagnosticResult.Id)] = false,
        };

        DataGridManager.QueryAsync = async () => await LoadData();
    }

    private async Task<List<Data>> LoadData()
    {
        await Task.CompletedTask;

        var result = new List<Data>();
        var execution = (await ExecutionsRepo.FirstOrDefaultAsync(new ExecutionSpec(string.Empty).ByKey(Id).Include()))!;
        var data = execution.Data.Data;
        if (data != null)
        {
            _clusterName = execution.ClusterName;
            var ignoreIssues = await Helper.GetIgnoredIssue(IgnoredIssuesRepo, execution.ClusterName);
            result = Application.Analyze(JsonConvert.DeserializeObject<InfoHelper.Info>(data),
                                         Options.Value.Get(execution.ClusterName),
                                         ignoreIssues)
                                .Where(a => !a.IsIgnoredIssue)
                                .AsQueryable()
                                .ProjectToType<Data>()
                                .ToList();

            foreach (var item in result)
            {
                switch (item.Context)
                {
                    case DiagnosticResultContext.Node:
                        switch (item.SubContext)
                        {
                            case "EOL": item.Url = "https://pve.proxmox.com/wiki/FAQ"; break;
                            default: break;
                        }
                        break;

                    case DiagnosticResultContext.Cluster: break;
                    case DiagnosticResultContext.Storage: break;

                    case DiagnosticResultContext.Qemu:
                        switch (item.SubContext)
                        {
                            case "Agent": item.Url = "https://pve.proxmox.com/wiki/Qemu-guest-agent"; break;

                            case "OSNotMaintained":
                                if (item.Description.Contains("Windows"))
                                {
                                    item.Url = "https://endoflife.date/windows";
                                }
                                else if (item.Description.Contains("Linux"))
                                {
                                    item.Url = "https://endoflife.date/linux";
                                }
                                break;

                            case "VirtIO":
                                if (item.Description.Contains("network"))
                                {
                                    item.Url = "https://pve.proxmox.com/pve-docs/chapter-qm.html#qm_network_device";
                                }
                                break;

                            default: break;
                        }
                        break;

                    case DiagnosticResultContext.Lxc: break;

                    default: break;
                }
            }
        }

        return result;
    }

    private async Task ShowInfo(Data item) => await BrowserService.Open(item.Url!, "_blank");

    private static string GroupClassFunc(GroupDefinition<Data> group)
        => (DiagnosticResultGravity)group.Grouping.Key switch
        {
            DiagnosticResultGravity.Info => "mud-theme-info",
            DiagnosticResultGravity.Warning => "mud-theme-warning",
            DiagnosticResultGravity.Critical => "mud-theme-error",
            _ => "mud-theme-info",
        };

    private async Task IgnoreIssue(Data item)
    {
        await IgnoredIssuesRepo.AddAsync(new()
        {
            ClusterName = _clusterName,
            IdResource = Regex.Escape(item.Id),
            Context = item.Context,
            Description = Regex.Escape(item.Description),
            SubContext = Regex.Escape(item.SubContext),
            Gravity = item.Gravity
        });

        //rescan
        Job.ScheduleRescan(JobService, _clusterName);
        UINotifier.Show(L["Rescan jobs started!"], UINotifierSeverity.Info);
    }
}