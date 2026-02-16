/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;

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

            var url = ReplacePlaceholders(hook.Url, variables);
            var body = ReplacePlaceholders(hook.Body, variables);

            var request = new HttpRequestMessage(ToHttpMethod(hook.Method), url);

            // Headers
            foreach (var (key, value) in hook.Headers)
            {
                request.Headers.TryAddWithoutValidation(key, ReplacePlaceholders(value, variables));
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

            var response = await client.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();
            var statusCode = (int)response.StatusCode;

            stopwatch.Stop();

            return new WebHookResult
            {
                Success = response.IsSuccessStatusCode,
                StatusCode = statusCode,
                ResponseBody = responseBody,
                DurationMs = stopwatch.ElapsedMilliseconds
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            logger.LogError(ex, "WebHook execution failed: {Url}", hook.Url);

            return new WebHookResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                DurationMs = stopwatch.ElapsedMilliseconds
            };
        }
    }

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

    private static string ReplacePlaceholders(string template, IReadOnlyDictionary<string, string> variables)
    {
        if (string.IsNullOrEmpty(template)) { return template; }

        var result = template;
        foreach (var (key, value) in variables)
        {
            result = result.Replace($"%{key}%", value ?? string.Empty);
        }
        return result;
    }
}
