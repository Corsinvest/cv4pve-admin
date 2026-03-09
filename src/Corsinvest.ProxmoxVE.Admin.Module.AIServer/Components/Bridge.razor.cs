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

    private static readonly PlatformInfo[] Platforms =
    [
        new("desktop_windows", "Windows x64", "cv4pve-mcp-bridge-win-x64.exe"),
        new("terminal", "Linux x64", "cv4pve-mcp-bridge-linux-x64"),
        new("terminal", "Linux ARM64", "cv4pve-mcp-bridge-linux-arm64"),
        new("laptop_mac", "macOS Intel", "cv4pve-mcp-bridge-osx-x64"),
        new("laptop_mac", "macOS Apple Silicon", "cv4pve-mcp-bridge-osx-arm64"),
    ];

    private string ApiToken { get; set; } = default!;
    private string McpUrl { get; set; } = default!;
    private bool Insecure { get; set; } = default!;
    private string BinaryPath { get; set; } = default!;

    protected override void OnInitialized()
    {
        var baseUri = new Uri(navigationManager.BaseUri);
        McpUrl = $"{baseUri.GetLeftPart(UriPartial.Authority)}/mcp";
        Insecure = baseUri.Scheme == "http";
    }

    private string BuildClaudeConfigJson()
    {
        var args = new List<string>
        {
            "--url",
            McpUrl,
            "--api-key",
            string.IsNullOrWhiteSpace(ApiToken)
                ? "YOUR-APP-TOKEN"
                : ApiToken
        };

        if (Insecure) { args.Add("--insecure"); }

        return JsonSerializer.Serialize(new
        {
            mcpServers = new Dictionary<string, object>
            {
                ["cv4pve-admin"] = new
                {
                    command = string.IsNullOrWhiteSpace(BinaryPath)
                                ? "/path/to/cv4pve-mcp-bridge"
                                : BinaryPath,
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
