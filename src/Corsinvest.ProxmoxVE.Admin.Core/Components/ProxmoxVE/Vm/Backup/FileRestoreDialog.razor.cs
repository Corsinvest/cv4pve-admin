/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Api.Shared.Models.Node;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components.ProxmoxVE.Vm.Backup;

public partial class FileRestoreDialog(IBrowserService browserService,
                                       NotificationService notificationService,
                                       IAdminService adminService,
                                       IFusionCache fusionCache) : IClusterName, INode
{
    [EditorRequired, Parameter] public string ClusterName { get; set; } = default!;
    [EditorRequired, Parameter] public string Node { get; set; } = default!;
    [EditorRequired, Parameter] public string Storage { get; set; } = default!;
    [EditorRequired, Parameter] public string Volume { get; set; } = default!;

    private bool IsLoading { get; set; }
    private RadzenDataGrid<NodeBackupFile> DataGridRef { get; set; } = default!;
    private IEnumerable<NodeBackupFile> Items { get; set; } = default!;
    private IList<NodeBackupFile> SelectedItems { get; set; } = [];
    private NodeBackupFile SelectedItem => SelectedItems[0];

    private const string Zip = ".zip";
    private const string TarZst = ".tar.zst";

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        base.OnAfterRender(firstRender);
        if (firstRender) { await RefreshDataAsync(); }
    }

    private async Task RefreshDataAsync()
    {
        await InvokeAsync(StateHasChanged);
        IsLoading = true;
        Items = await GetItemsBackups(string.Empty);
        SelectedItems.Clear();
        await DataGridRef.ExpandRows(Items);
        IsLoading = false;
        await InvokeAsync(StateHasChanged);
    }

    private static void RowRender(RowRenderEventArgs<NodeBackupFile> args)
        => args.Expandable = args.Data!.Type switch
        {
            "v" or "d" => true,
            "l" or "f" => false,
            _ => false
        };

    private async Task LoadChildData(DataGridLoadChildDataEventArgs<NodeBackupFile> args)
    {
        await InvokeAsync(StateHasChanged);
        IsLoading = true;

        try
        {
            args.Data = await GetItemsBackups(args.Item!.FilePath);
        }
        catch (Exception ex)
        {
            notificationService.Error(L["Error expand item"], ex.Message);
        }

        IsLoading = false;
        await InvokeAsync(StateHasChanged);
    }

    private async Task<IEnumerable<NodeBackupFile>> GetItemsBackups(string filePath)
    {
        var client = await adminService[ClusterName].GetPveClientAsync();
        return [.. (await client.Nodes[Node]
                                .Storage[Storage]
                                .FileRestore
                                .List
                                .GetAsync(filepath: filePath, volume: Volume))
                .OrderBy(a=> a.Type)];
    }

    private async Task DownloadFolderAsync(RadzenSplitButtonItem item)
    {
        switch (item.Value)
        {
            case Zip: await DownloadAsync(false); break;
            case TarZst: await DownloadAsync(true); break;
            default: break;
        }
    }

    private async Task DownloadFileAsync() => await DownloadAsync(false);

    private async Task DownloadAsync(bool tar)
        => await browserService.OpenAsync(PveAdminHelper.GetUrlDowloadFileBackup(fusionCache,
                                                                                 ClusterName,
                                                                                 Node,
                                                                                 Storage,
                                                                                 Volume,
                                                                                 SelectedItem.FilePath,
                                                                                 SelectedItem.Type,
                                                                                 SelectedItem.Text,
                                                                                 tar)
                                         , "_blank");

    private static string GetIcon(NodeBackupFile item)
        => item.Type switch
        {
            "v" => "folder",
            "d" => "folder",
            "l" => "link",
            "f" => GetIconFile(item.Text),
            _ => null!
        };

    private static string GetIconFile(string fileName)
            => fileName.Split('.').Last().ToLower() switch
            {
                _ => "description"
            };
}
