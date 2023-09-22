/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using BlazorDownloadFile;
using Corsinvest.ProxmoxVE.Admin.Core.Services;
using Corsinvest.ProxmoxVE.Admin.Diagnostic.Repository;

namespace Corsinvest.ProxmoxVE.Admin.Diagnostic.Components;

public partial class Results
{
    [Parameter] public string Height { get; set; } = default!;

    [Inject] private IPveClientService PveClientService { get; set; } = default!;
    [Inject] private IBlazorDownloadFileService BlazorDownloadFileService { get; set; } = default!;
    [Inject] private IReadRepository<IgnoredIssue> IgnoredIssuesRepo { get; set; } = default!;
    [Inject] private IReadRepository<Execution> ExecutionsRepo { get; set; } = default!;
    [Inject] private IDataGridManagerRepository<Execution> DataGridManager { get; set; } = default!;
    [Inject] private IJobService JobService { get; set; } = default!;
    [Inject] private IOptionsSnapshot<Options> Options { get; set; } = default!;
    [Inject] private IOptionsSnapshot<AppOptions> AppOptions { get; set; } = default!;
    private string ClusterName { get; set; } = default!;
    private bool InDownloadPdf { get; set; }

    protected override async Task OnInitializedAsync()
    {
        DataGridManager.Title = L["Results"];
        DataGridManager.DefaultSort = new() { [nameof(Execution.Date)] = true };
        DataGridManager.QueryAsync = async () => await DataGridManager.Repository.ListAsync(new ExecutionSpec(ClusterName));

        try
        {
            ClusterName = await PveClientService.GetCurrentClusterNameAsync();
        }
        catch { }
    }

    private void Run()
    {
        JobService.Schedule<Job>(a => a.Create(ClusterName), TimeSpan.FromSeconds(10));
        UINotifier.Show(L["Diagnostic jobs started!"], UINotifierSeverity.Info);
    }

    private async Task DownloadPdf(Execution item)
    {
        InDownloadPdf = true;

        var ignoreIssues = await Helper.GetIgnoredIssue(IgnoredIssuesRepo, ClusterName);
        var data = (await ExecutionsRepo.FirstOrDefaultAsync(new ExecutionSpec(ClusterName).ByKey(item.Id).Include()))!.Data;
        using var ms = Helper.GeneratePdf(L,
                                          AppOptions.Value,
                                          item,
                                          Helper.Analyze(data, Options.Value.Get(ClusterName), ignoreIssues),
                                          ClusterName)!;

        await BlazorDownloadFileService.DownloadFile("Diagnostic.pdf", ms, System.Net.Mime.MediaTypeNames.Application.Pdf);

        InDownloadPdf = false;
    }
}