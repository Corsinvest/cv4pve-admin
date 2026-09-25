/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Security;
using System.Text;
using System.Text.Json;

namespace Corsinvest.ProxmoxVE.Admin.Core.Hooks;

internal class HookExecutor(IHttpClientFactory httpClientFactory, ILogger<HookExecutor> logger) : IHookExecutor
{
    public async Task<WebHookResult> ExecuteAsync(WebHook hook, IReadOnlyDictionary<string, string> variables)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var clientName = hook.IgnoreSslCertificate
                                ? "HttpIgnoreCert"
                                : "HttpStrict";
            var client = httpClientFactory.CreateClient(clientName);
            client.Timeout = TimeSpan.FromSeconds(hook.TimeoutSeconds);

            // Each value is escaped for where it lands, so a quote or a line break in a job log
            // cannot break the URL, a header or the body syntax
            var url = ReplacePlaceholders(hook.Url, variables, Uri.EscapeDataString);
            Func<string, string> encodeBody = hook.BodyType switch
            {
                WebHookBodyType.Json => value => JsonEncodedText.Encode(value).ToString(),
                WebHookBodyType.Xml => value => SecurityElement.Escape(value) ?? string.Empty,
                _ => value => value
            };
            var body = ReplacePlaceholders(hook.Body, variables, encodeBody);

            using var request = new HttpRequestMessage(ToHttpMethod(hook.Method), url);

            // Headers: a line break would end the header, so it becomes a space
            foreach (var (key, value) in hook.Headers)
            {
                request.Headers.TryAddWithoutValidation(key, ReplacePlaceholders(value, variables, a => a.ReplaceLineEndings(" ")));
            }

            // Auth
            ApplyAuth(request, hook.Auth);

            // Body
            if (hook.BodyType != WebHookBodyType.None && !string.IsNullOrEmpty(body) && hook.Method != WebHookHttpMethod.Get)
            {
                var mediaType = hook.BodyType switch
                {
                    WebHookBodyType.Json => "application/json",
                    WebHookBodyType.Xml => "application/xml",
                    WebHookBodyType.Text => "text/plain",
                    _ => "application/json"
                };
                request.Content = new StringContent(body, Encoding.UTF8, mediaType);
            }

            using var response = await client.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();
            var statusCode = (int)response.StatusCode;

            stopwatch.Stop();

            // The receiver's answer usually says why it refused the call: keep a short part of it
            string? errorMessage = null;
            if (!response.IsSuccessStatusCode)
            {
                errorMessage = $"HTTP {statusCode} {response.ReasonPhrase}";
                if (!string.IsNullOrWhiteSpace(responseBody))
                {
                    errorMessage += $": {(responseBody.Length > MaxErrorBodyLength ? responseBody[..MaxErrorBodyLength] + "..." : responseBody)}";
                }
                logger.LogWarning("WebHook call to {Target} failed: {Error}", SafeTarget(url), errorMessage);
            }

            return new WebHookResult
            {
                Success = response.IsSuccessStatusCode,
                StatusCode = statusCode,
                ResponseBody = responseBody,
                ErrorMessage = errorMessage,
                DurationMs = stopwatch.ElapsedMilliseconds
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            logger.LogError(ex, "WebHook execution failed: {Target}", SafeTarget(hook.Url));

            return new WebHookResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                DurationMs = stopwatch.ElapsedMilliseconds
            };
        }
    }

    private const int MaxErrorBodyLength = 500;

    // Scheme and host only: Slack, Discord and Teams put the secret in the path or the query
    private static string SafeTarget(string url)
        => Uri.TryCreate(url, UriKind.Absolute, out var uri)
            ? $"{uri.Scheme}://{uri.Authority}"
            : "(invalid URL)";

    private static void ApplyAuth(HttpRequestMessage request, WebHookAuth auth)
    {
        switch (auth.Type)
        {
            case WebHookAuthType.Basic:
                var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{auth.Username}:{auth.Password}"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
                break;

            case WebHookAuthType.Bearer:
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);
                break;

            case WebHookAuthType.ApiKey:
                request.Headers.TryAddWithoutValidation(auth.ApiKeyHeader, auth.ApiKeyValue);
                break;
        }
    }

    private static HttpMethod ToHttpMethod(WebHookHttpMethod method)
        => method switch
        {
            WebHookHttpMethod.Get => HttpMethod.Get,
            WebHookHttpMethod.Post => HttpMethod.Post,
            WebHookHttpMethod.Put => HttpMethod.Put,
            WebHookHttpMethod.Patch => HttpMethod.Patch,
            WebHookHttpMethod.Delete => HttpMethod.Delete,
            _ => HttpMethod.Post
        };

    // Single pass: a value containing "%name%" is not expanded again.
    // An unknown "%...%" is kept and its closing '%' can still open a placeholder, so a URL
    // like "a%20%subject%" (percent-encoding next to a placeholder) is expanded correctly.
    private static string ReplacePlaceholders(string template,
                                              IReadOnlyDictionary<string, string> variables,
                                              Func<string, string> encode)
    {
        if (string.IsNullOrEmpty(template)) { return template; }

        var result = new StringBuilder(template.Length);
        var i = 0;
        while (i < template.Length)
        {
            var end = template[i] == '%'
                        ? template.IndexOf('%', i + 1)
                        : -1;

            if (end > i + 1 && variables.TryGetValue(template[(i + 1)..end], out var value))
            {
                result.Append(encode(value ?? string.Empty));
                i = end + 1;
            }
            else
            {
                result.Append(template[i]);
                i++;
            }
        }

        return result.ToString();
    }
}
