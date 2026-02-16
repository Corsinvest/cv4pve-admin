/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
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

        var variables = new Dictionary<string, string>
        {
            ["subject"] = message.Subject,
            ["body"] = message.Body
        };

        var hookExecutor = serviceProvider.GetRequiredService<IHookExecutor>();
        var result = await hookExecutor.ExecuteAsync(hook, variables);

        return result.Success
            ? OperationResult.Ok()
            : OperationResult.Fail(result.ErrorMessage ?? $"HTTP {result.StatusCode}");
    }
}
