/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using BlazorDownloadFile;
using Microsoft.JSInterop;

namespace Corsinvest.ProxmoxVE.Admin.Core.Services;

public class BrowserService(IJSRuntime jSRuntime, IBlazorDownloadFileService blazorDownloadFileService) : IBrowserService
{
    public async Task CopyToClipboardAsync(string text) => await jSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", text);
    public async Task OpenAsync(string url, string target) => await jSRuntime.InvokeVoidAsync("open", url, target);
    public async Task OpenInNewWindowAsync(string url, string windowFeatures)
        => await jSRuntime.InvokeVoidAsync("eval", $"window.open('{url}', '_blank', '{windowFeatures}')");
    public async Task DownloadFileAsync(string fileName, string content, string mimeType)
        => await blazorDownloadFileService.DownloadFileFromText(fileName, content, System.Text.Encoding.UTF8, mimeType);
    public async Task DownloadFileAsync(string fileName, Stream stream, string mimeType)
        => await blazorDownloadFileService.DownloadFile(fileName, stream, mimeType);
}
