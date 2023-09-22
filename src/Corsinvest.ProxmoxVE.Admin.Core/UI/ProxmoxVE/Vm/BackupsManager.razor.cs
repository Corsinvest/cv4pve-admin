/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.Security.Auth.Permissions;
using Corsinvest.AppHero.Core.Service;
using Corsinvest.ProxmoxVE.Api;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Node;
using Corsinvest.ProxmoxVE.Api.Shared.Utils;

namespace Corsinvest.ProxmoxVE.Admin.Core.UI.ProxmoxVE.Vm;

public partial class BackupsManager
{
    [EditorRequired][Parameter] public IClusterResourceVm Vm { get; set; } = default!;
    [EditorRequired][Parameter] public PveClient PveClient { get; set; } = default!;
    [Parameter] public Func<NodeStorageContent, NodeBackupFile, string> GetUrlRestoreFile { get; set; } = default!;
    [Parameter] public PermissionsRead Permissions { get; set; } = default!;
    [Parameter] public string Height { get; set; } = default!;
    [Parameter] public bool CanRestoreFile { get; set; }
    [Parameter] public bool ShowDetailProxmoxVE { get; set; }

    [Inject] private IBrowserService BrowserService { get; set; } = default!;
    [Inject] private IDataGridManager<NodeStorageContent> DataGridManager { get; set; } = default!;

    private bool DialogVisible { get; set; }
    private NodeBackupFile? NodeBackupFileToRestore { get; set; }

    protected override void OnInitialized()
    {
        DataGridManager.Title = L["Backups"];
        DataGridManager.QueryAsync = async () => await PveClient.Nodes[Vm.Node].GetBackupsInAllStorages(Convert.ToInt32(Vm.VmId));
    }

    private void SelectedFileChanged(NodeBackupFile nodeBackupFile) => NodeBackupFileToRestore = nodeBackupFile;
    private bool BackupIsRestorable() => !DataGridManager.ExistsSelection || DataGridManager.SelectedItems.ToArray()[0].Format == "PBS";

    private async Task DownloadFileRestore()
    {
        if (NodeBackupFileToRestore == null) { return; }
        var storageContent = DataGridManager.SelectedItem;

        //check big file > 100Mb
        if (!((storageContent.Size / 1024 / 1024) > 50 &&
            await UIMessageBox.ShowQuestionAsync(L["Download file"], L["Download file?"])))
        {
            return;
        }

        var url = GetUrlRestoreFile == null
                    ? BackupHelper.GetDownloadFileUrl(PveClient.Host,
                                                      PveClient.Port,
                                                      Vm.Node,
                                                      storageContent.Storage,
                                                      storageContent.Volume,
                                                      NodeBackupFileToRestore.FilePath)
                    : GetUrlRestoreFile.Invoke(storageContent, NodeBackupFileToRestore);

        await BrowserService.Open(url, "_blank");
    }

    private async Task<HashSet<NodeBackupFile>> GetItemsBackups(NodeBackupFile nodeBackupFile)
        => (await PveClient.Nodes[Vm.Node]
            .Storage[DataGridManager.SelectedItem.Storage]
            .FileRestore.List.Get(filepath: nodeBackupFile.FilePath, volume: DataGridManager.SelectedItem.Volume))
            .OrderBy(a => a.Text)
            .ToHashSet();

    private string RowClassFunc(NodeStorageContent item, int rowNumber)
        => DataGridManager.SelectedItem == item
            ? "selected"
            : string.Empty;
}