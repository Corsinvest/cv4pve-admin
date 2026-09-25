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
        var hook = WebHook.Clone();
        if (string.IsNullOrEmpty(hook.Body)) { hook.Body = """{"subject": "%subject%", "body": "%body%"}"""; }

        // Raw values: the executor escapes each one for where it lands (URL, header, JSON or XML body)
        var variables = new Dictionary<string, string>
        {
            ["subject"] = message.Subject ?? string.Empty,
            ["body"] = message.Body ?? string.Empty,
            ["severity"] = message.Severity.ToString(),
        };

        var hookExecutor = serviceProvider.GetRequiredService<IHookExecutor>();
        var result = await hookExecutor.ExecuteAsync(hook, variables);

        return result.Success
            ? OperationResult.Ok()
            : OperationResult.Fail(result.ErrorMessage ?? $"HTTP {result.StatusCode}");
    }
}
