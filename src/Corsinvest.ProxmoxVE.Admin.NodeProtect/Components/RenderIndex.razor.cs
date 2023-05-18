/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using BlazorDownloadFile;
using Corsinvest.AppHero.Core.BaseUI.DataManager;
using Corsinvest.AppHero.Core.UI;
using Corsinvest.ProxmoxVE.Admin.Core.Repository;
using Corsinvest.ProxmoxVE.Admin.Core.Services;
using System.Net.Mime;

namespace Corsinvest.ProxmoxVE.Admin.NodeProtect.Components;

public partial class RenderIndex
{
    [Inject] private IBlazorDownloadFileService BlazorDownloadFileService { get; set; } = default!;
    [Inject] private IDataGridManagerRepository<NodeProtectJobHistory> DataGridManager { get; set; } = default!;
    [Inject] private IPveClientService PveClientService { get; set; } = default!;
    [Inject] private IServiceScopeFactory ScopeFactory { get; set; } = default!;
    [Inject] private IJobService JobService { get; set; } = default!;
    [Inject] private IOptionsSnapshot<Options> Options { get; set; } = default!;

    private bool ShowDialog { get; set; }
    private bool LoadingDownload { get; set; }
    private bool LoadingUpload { get; set; }
    private string DialogContent { get; set; } = default!;
    private string ClusterName { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            ClusterName = await PveClientService.GetCurrentClusterName();
        }
        catch { }

        DataGridManager.Title = L["Node protected"];
        DataGridManager.DefaultSort = new() { [nameof(NodeProtectJobHistory.JobId)] = true };
        DataGridManager.QueryAsync = async () => await DataGridManager.Repository.ListAsync(new ClusterByNameSpec<NodeProtectJobHistory>(ClusterName));

        DataGridManager.DeleteAfterAsync = async (items) =>
        {
            foreach (var item in items)
            {
                //delete file
                if (File.Exists(item.GetPath())) { File.Delete(item.GetPath()); }

                //remove folder id empty
                if (!Directory.GetFiles(item.GetDirectoryWorkJobId()).Any())
                {
                    Directory.Delete(item.GetDirectoryWorkJobId(), true);
                }
            }

            await Task.CompletedTask;
        };
    }

    private async Task Protect()
    {
        if (await UIMessageBox.ShowQuestionAsync(L["Protect"], L["Execute Protect?"]))
        {
            JobService.Schedule<Job>(a => a.Protect(ClusterName), TimeSpan.FromSeconds(10));
            UINotifier.Show(L["Protect started!"], UINotifierSeverity.Info);
        }
    }

    private async Task Download(NodeProtectJobHistory item)
    {
        LoadingDownload = true;
        if (File.Exists(item.GetPath()))
        {
            using var fs = File.OpenRead(item.GetPath());
            await BlazorDownloadFileService.DownloadFile(item.FileName, fs, MediaTypeNames.Application.Zip);
        }
        LoadingDownload = false;
    }

    private async Task Upload(NodeProtectJobHistory item)
    {
        LoadingUpload = true;
        if (await UIMessageBox.ShowQuestionAsync(L["Upload into node"], L["Confirm upload to node in /tmp?"]))
        {
            using var scope = ScopeFactory.CreateScope();
            Helper.UploadToNode(scope, item);
        }
        LoadingUpload = false;
    }

    private void ShowLog(NodeProtectJobHistory item)
    {
        DialogContent = item.Log;
        ShowDialog = true;
    }
}