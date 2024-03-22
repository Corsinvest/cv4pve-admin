/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Api.Shared.Models.Node;
using Corsinvest.ProxmoxVE.Api.Shared.Utils;

namespace Corsinvest.ProxmoxVE.Admin.Core.UI.ProxmoxVE.Vm;

public partial class BackupFileRestore
{
    [Parameter] public Func<NodeBackupFile, Task<HashSet<NodeBackupFile>>> GetItems { get; set; } = default!;
    [Parameter] public EventCallback<NodeBackupFile> SelectedFileChanged { get; set; }

    static string GetEndText(NodeBackupFile item)
    {
        var text = new List<string>();
        if (item.Size > 0) { text.Add(FormatHelper.FromBytes(item.Size)); }
        if (item.ModifiedTime > 0) { text.Add($"{DateTimeOffset.FromUnixTimeSeconds(item.ModifiedTime).DateTime:R}"); }
        return string.Join(" | ", text);
    }

    private static string GetIcon(NodeBackupFile item)
        => item.Type switch
        {
            "v" => Icons.Material.Filled.Folder,
            "d" => Icons.Material.Filled.Folder,
            "f" => GetIconFile(item.Text),
            _ => null!,
        };

    static string GetIconFile(string fileName)
        => fileName.Split('.').Last().ToLower() switch
        {
            //excel
            ".xlsx" or ".xlsm" or ".xlsb" or ".xltx" or ".xltm" or ".xls" or ".xlt" or ".xls" or ".xlam" or
            ".xla" or ".xlw" or ".xlr" => Icons.Custom.FileFormats.FileExcel,

            //word
            ".doc" or ".docm" or ".docx" or ".docx" or ".dot" or ".dotm" or ".dotx" or ".odt" or ".rtf" => Icons.Custom.FileFormats.FileWord,

            "pdf" => Icons.Custom.FileFormats.FilePdf,

            //image
            ".jpg" or ".jpeg" or ".jpe" or ".jif" or ".jfif" or ".jfi" or ".png" or ".gif" or ".webp" or
            ".tiff" or ".tif" or ".psd" or ".raw" or ".arw" or ".cr2" or ".nrw" or ".k25" or ".bmp" or
            ".dib" or ".heif" or ".heic" or ".ind" or ".indd" or ".indt" or ".jp2" or ".j2k" or ".jpf" or
            ".jpx" or ".jpm" or ".mj2" or ".svg" or ".svgz" or ".ai" or ".eps" => Icons.Custom.FileFormats.FileImage,

            //file video
            ".webm" or ".mkv" or ".flv" or ".flv" or ".vob" or ".ogv" or ".ogg" or ".drc" or ".gif" or
            ".mng" or ".avi" or ".mts" or ".m2ts" or ".ts" or ".mov" or ".qt" or ".wmv" or ".yuv" or
            ".rm" or ".rmvb" or ".viv" or ".asf" or ".amv" or ".mp4" or ".m4p" or ".m4v" or ".mpg" or
            ".mp2" or ".mpeg" or ".mpe" or ".mpv" or ".mpg" or ".mpeg" or ".m2v" or ".m4v" or ".svi" or
            ".3gp" or ".3g2" or ".mxf" or ".roq" or ".nsv" or ".flv" or ".f4v" or ".f4p" or ".f4a" or
            ".f4b" => Icons.Custom.FileFormats.FileVideo,

            _ => Icons.Material.Filled.Description,
        };

    HashSet<NodeBackupFile> TreeItems { get; set; } = [];

    protected override async void OnInitialized()
    {
        TreeItems = await LoadServerData(new NodeBackupFile
        {
            FilePath = "/",
        });
        StateHasChanged();
    }

    private async Task<HashSet<NodeBackupFile>> LoadServerData(NodeBackupFile parentNode)
        => parentNode.Type == "f"
                ? null!
                : await GetItems(parentNode);
}