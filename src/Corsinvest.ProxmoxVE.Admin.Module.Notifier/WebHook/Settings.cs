/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Text.Json;
using Corsinvest.ProxmoxVE.Admin.Core.Hooks;
using Microsoft.Extensions.DependencyInjection;
using OperationResult = FluentResults.Result;

namespace Corsinvest.ProxmoxVE.Admin.Module.Notifier.WebHook;

public class Settings : NotifierConfiguration
{
    public Core.Hooks.WebHook WebHook { get; set; } = new();

    protected override async Task<OperationResult> SendImpAsync(NotifierMessage message, IServiceProvider serviceProvider)
    {
        var hook = new Core.Hooks.WebHook
        {
            Url = WebHook.Url,
            Method = WebHook.Method,
            Headers = WebHook.Headers,
            BodyType = WebHook.BodyType,
            IgnoreSslCertificate = WebHook.IgnoreSslCertificate,
            TimeoutSeconds = WebHook.TimeoutSeconds,
            Auth = WebHook.Auth,
            Body = string.IsNullOrEmpty(WebHook.Body)
                ? """{"subject": "%subject%", "body": "%body%"}"""
                : WebHook.Body
        };

        // The values land inside the template as-is, so for a JSON body they are escaped first:
        // a body carrying a quote or a line break — a job log, a list of UPS alerts — would
        // otherwise produce a payload the receiver rejects as malformed. Left untouched for the
        // other body types, where JSON escaping would be wrong.
        string Encode(string? value) => hook.BodyType == WebHookBodyType.Json
            ? JsonEncodedText.Encode(value ?? string.Empty).ToString()
            : value ?? string.Empty;

        var variables = new Dictionary<string, string>
        {
            ["subject"] = Encode(message.Subject),
            ["body"] = Encode(message.Body),
            ["severity"] = message.Severity.ToString(),
        };

        var hookExecutor = serviceProvider.GetRequiredService<IHookExecutor>();
        var result = await hookExecutor.ExecuteAsync(hook, variables);

        return result.Success
            ? OperationResult.Ok()
            : OperationResult.Fail(result.ErrorMessage ?? $"HTTP {result.StatusCode}");
    }
}
