/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Microsoft.JSInterop;

namespace Corsinvest.ProxmoxVE.Admin.Core.Services;

public class BrowserService(IJSRuntime jSRuntime) : IBrowserService
{
    public async Task CopyToClipboardAsync(string text) => await jSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", text);
    public async Task OpenAsync(string url, string target) => await jSRuntime.InvokeVoidAsync("open", url, target);
    public async Task OpenInNewWindowAsync(string url, string windowFeatures)
        => await jSRuntime.InvokeVoidAsync("eval", $"window.open('{url}', '_blank', '{windowFeatures}')");
}
