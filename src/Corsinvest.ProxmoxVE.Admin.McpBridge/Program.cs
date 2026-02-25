/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.CommandLine;
using ModelContextProtocol.Client;
using ModelContextProtocol.Server;

var urlOption = new Option<string>("--url", ["-u"])
{
    Description = "MCP server endpoint URL. Env: MCP_URL",
    DefaultValueFactory = (_) => Environment.GetEnvironmentVariable("MCP_URL") ?? string.Empty
};

var apiKeyOption = new Option<string>("--api-key", ["-k"])
{
    Description = "API key for authentication. Env: MCP_API_KEY",
    DefaultValueFactory = (_) => Environment.GetEnvironmentVariable("MCP_API_KEY") ?? string.Empty
};

var insecureOption = new Option<bool>("--insecure", ["-i"])
{
    Description = "Disable SSL certificate validation. DANGEROUS: use only in development. Env: MCP_INSECURE",
    DefaultValueFactory = (_) => Environment.GetEnvironmentVariable("MCP_INSECURE") is "1" or "true"
};

var rootCommand = new RootCommand() { urlOption, apiKeyOption, insecureOption };

rootCommand.Description = @"
   ______                _                      __
  / ____/___  __________(_)___ _   _____  _____/ /_
 / /   / __ \/ ___/ ___/ / __ \ | / / _ \/ ___/ __/
/ /___/ /_/ / /  (__  ) / / / / |/ /  __(__  ) /_
\____/\____/_/  /____/_/_/ /_/|___/\___/____/\__/

Bridge for MCP
For more information visit https://github.com/Corsinvest";

rootCommand.SetAction(async action =>
{
    var mcpUrl = action.GetValue(urlOption);
    var apiKey = action.GetValue(apiKeyOption);
    var insecure = action.GetValue(insecureOption);

    if (string.IsNullOrWhiteSpace(mcpUrl) || string.IsNullOrWhiteSpace(apiKey))
    {
        if (string.IsNullOrWhiteSpace(mcpUrl)) { Console.Error.WriteLine("❌ Error: MCP_URL is not set via argument or environment variable."); }
        if (string.IsNullOrWhiteSpace(apiKey)) { Console.Error.WriteLine("❌ Error: MCP_API_KEY is not set via argument or environment variable."); }
        Environment.Exit(1);
    }

    if (insecure) { Console.Error.WriteLine("⚠️  WARNING: SSL validation is DISABLED (--insecure)"); }

    var handler = insecure
        ? new HttpClientHandler { ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator }
        : new HttpClientHandler();

    var httpTransport = new HttpClientTransport(
        new HttpClientTransportOptions
        {
            Endpoint = new Uri(mcpUrl!),
            AdditionalHeaders = new Dictionary<string, string> { ["X-API-Key"] = apiKey! },
            TransportMode = HttpTransportMode.StreamableHttp
        },
        new HttpClient(handler));

    Console.Error.WriteLine($"[Bridge] Connecting to {mcpUrl}");

    await using var remoteTransport = await httpTransport.ConnectAsync();
    await using var stdioTransport = new StdioServerTransport("cv4pve-mcp-bridge");

    Console.Error.WriteLine("[Bridge] Connected");

    // Proxy stdio → remote
    var stdioToRemote = Task.Run(async () =>
    {
        await foreach (var message in stdioTransport.MessageReader.ReadAllAsync())
        {
            await remoteTransport.SendMessageAsync(message);
        }
    });

    // Proxy remote → stdio
    var remoteToStdio = Task.Run(async () =>
    {
        await foreach (var message in remoteTransport.MessageReader.ReadAllAsync())
        {
            await stdioTransport.SendMessageAsync(message);
        }
    });

    await Task.WhenAny(stdioToRemote, remoteToStdio);
});

return await rootCommand.Parse(args).InvokeAsync();
