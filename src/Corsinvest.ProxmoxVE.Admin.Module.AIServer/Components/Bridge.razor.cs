/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Net.Mime;
using System.Text.Json;
using BlazorDownloadFile;
using Microsoft.AspNetCore.Components;

namespace Corsinvest.ProxmoxVE.Admin.Module.AIServer.Components;

public partial class Bridge(NavigationManager navigationManager,
                            IBrowserService browserService,
                            IBlazorDownloadFileService blazorDownloadFileService)
{
    private static readonly JsonSerializerOptions jsonSerializerOptions = new() { WriteIndented = true };
    private record PlatformInfo(string Icon, string Platform, string File);

    private static readonly PlatformInfo[] _platforms =
    [
        new("desktop_windows", "Windows x64", "cv4pve-mcp-bridge-win-x64.exe"),
        new("terminal", "Linux x64", "cv4pve-mcp-bridge-linux-x64"),
        new("terminal", "Linux ARM64", "cv4pve-mcp-bridge-linux-arm64"),
        new("laptop_mac", "macOS Intel", "cv4pve-mcp-bridge-osx-x64"),
        new("laptop_mac", "macOS Apple Silicon", "cv4pve-mcp-bridge-osx-arm64"),
    ];

    private string _mcpUrl = string.Empty;
    private bool _insecure;
    private string _binaryPath = string.Empty;
    private string _apiToken = string.Empty;

    protected override void OnInitialized()
    {
        var baseUri = new Uri(navigationManager.BaseUri);
        _mcpUrl = $"{baseUri.GetLeftPart(UriPartial.Authority)}/mcp";
        _insecure = baseUri.Scheme == "http";
    }

    private Task DownloadBinaryAsync(PlatformInfo platform)
        => browserService.OpenAsync($"{ApplicationHelper.GitHubReleasesVersionDownloadUrl}/{platform.File}", "_blank");

    private string BuildClaudeConfigJson()
    {
        var args = new List<string>
        {
            "--url",
            _mcpUrl,
            "--api-key",
            string.IsNullOrWhiteSpace(_apiToken)
                ? "YOUR-APP-TOKEN"
                : _apiToken
        };

        if (_insecure) { args.Add("--insecure"); }

        return JsonSerializer.Serialize(new
        {
            mcpServers = new Dictionary<string, object>
            {
                ["cv4pve-admin"] = new
                {
                    command = string.IsNullOrWhiteSpace(_binaryPath)
                                ? "/path/to/cv4pve-mcp-bridge"
                                : _binaryPath,
                    args
                }
            }
        },
        jsonSerializerOptions);
    }

    private async Task DownloadClaudeConfigAsync()
        => await blazorDownloadFileService.DownloadFile("claude_desktop_config.json",
                                                        System.Text.Encoding.UTF8.GetBytes(BuildClaudeConfigJson()),
                                                        MediaTypeNames.Application.Json);

    private Task CopyClaudeConfigAsync()
        => browserService.CopyToClipboardAsync(BuildClaudeConfigJson());
}
