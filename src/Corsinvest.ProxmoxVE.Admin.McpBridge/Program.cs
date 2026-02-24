/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.CommandLine;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

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
        if (string.IsNullOrWhiteSpace(mcpUrl))
        {
            Console.Error.WriteLine("❌ Error: MCP_URL is not set via argument or environment variable.");
        }

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            Console.Error.WriteLine("❌ Error: MCP_API_KEY is not set via argument or environment variable.");

        }
        Environment.Exit(1);
    }

    // --------------------------------------------------
    // SSL handling
    // --------------------------------------------------

    if (insecure) { Console.Error.WriteLine("⚠️  WARNING: SSL validation is DISABLED (--insecure)"); }

    var handler = insecure
        ? new HttpClientHandler { ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator }
        : new HttpClientHandler();

    // --------------------------------------------------
    // HttpClient setup
    // --------------------------------------------------

    using var httpClient = new HttpClient(handler)
    {
        Timeout = TimeSpan.FromMinutes(5)
    };

    httpClient.DefaultRequestHeaders.Add("X-API-Key", apiKey);
    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

    // --------------------------------------------------
    // STDIN reader
    // --------------------------------------------------
    using var reader = new StreamReader(Console.OpenStandardInput(), Encoding.UTF8);
    var buffer = new StringBuilder();
    Console.Error.WriteLine($"[Bridge] Connected to {mcpUrl}");

    while (await reader.ReadLineAsync() is { } line)
    {
        buffer.Append(line);

        string json;

        // --------------------------------------------------
        // Wait until JSON is complete
        // --------------------------------------------------

        try
        {
            JsonDocument.Parse(buffer.ToString());
            json = buffer.ToString();
            buffer.Clear();
        }
        catch (JsonException)
        {
            continue;
        }

        Console.Error.WriteLine($"[Bridge] Sending: {json}");

        try
        {
            // --------------------------------------------------
            // Send request
            // --------------------------------------------------

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await httpClient.PostAsync(mcpUrl, content);

            // --------------------------------------------------
            // Check response
            // --------------------------------------------------

            if (!response.IsSuccessStatusCode)
            {
                var statusCode = (int)response.StatusCode;
                var errorBody = await response.Content.ReadAsStringAsync();
                Console.Error.WriteLine($"[Bridge] HTTP {statusCode} {response.ReasonPhrase}: {errorBody}");

                // Extract id from request to build a valid JSON-RPC error response
                string? requestId = null;
                try
                {
                    using var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("id", out var idEl))
                    {
                        requestId = idEl.ValueKind == JsonValueKind.String ? $"\"{idEl.GetString()}\"" : idEl.GetRawText();
                    }
                }
                catch { }

                // Extract error message from server response if possible
                string errorMessage = $"HTTP {statusCode}: {response.ReasonPhrase}";
                try
                {
                    using var doc = JsonDocument.Parse(errorBody);
                    if (doc.RootElement.TryGetProperty("error", out var errEl))
                    {
                        errorMessage = errEl.GetString() ?? errorMessage;
                    }
                }
                catch { }

                var escapedMessage = errorMessage.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
                var errorResponse = $"{{\"jsonrpc\":\"2.0\",\"id\":{requestId ?? "null"},\"error\":{{\"code\":{-statusCode},\"message\":\"{escapedMessage}\"}}}}";
                await Console.Out.WriteLineAsync(errorResponse);
                await Console.Out.FlushAsync();
                continue;
            }

            // --------------------------------------------------
            // Read SSE stream
            // --------------------------------------------------

            await using var responseStream = await response.Content.ReadAsStreamAsync();
            using var responseReader = new StreamReader(responseStream, Encoding.UTF8
            );

            while (await responseReader.ReadLineAsync() is { } chunk)
            {
                if (!chunk.StartsWith("data: ")) { continue; }
                var jsonData = chunk["data: ".Length..].Trim();

                try
                {
                    // Validate JSON
                    JsonDocument.Parse(jsonData);

                    // Forward to stdout
                    await Console.Out.WriteLineAsync(jsonData);
                    await Console.Out.FlushAsync();
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"[Bridge] SSE Parse Error: {ex.Message}");
                }
            }
        }
        catch (Exception ex) { Console.Error.WriteLine($"[Bridge] HTTP Error: {ex.Message}"); }
    }
});

return await rootCommand.Parse(args).InvokeAsync();
