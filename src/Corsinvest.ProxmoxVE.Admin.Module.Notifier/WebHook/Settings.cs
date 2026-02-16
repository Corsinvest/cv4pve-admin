/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Hooks;
using Corsinvest.ProxmoxVE.Admin.Core.Notifier;
using Microsoft.Extensions.DependencyInjection;
using OperationResult = FluentResults.Result;

namespace Corsinvest.ProxmoxVE.Admin.Module.Notifier.WebHook;

public class Settings : NotifierConfiguration
{
    [Required] public string Url { get; set; } = default!;
    public WebHookHttpMethod Method { get; set; } = WebHookHttpMethod.Post;
    public WebHookBodyType BodyType { get; set; } = WebHookBodyType.Json;
    public string Body { get; set; } = string.Empty;
    public Dictionary<string, string> Headers { get; set; } = [];
    public bool IgnoreSslCertificate { get; set; }
    public int TimeoutSeconds { get; set; } = 30;
    public WebHookAuth Auth { get; set; } = new();

    protected override async Task<OperationResult> SendImpAsync(NotifierMessage message, IServiceProvider serviceProvider)
    {
        var hookExecutor = serviceProvider.GetRequiredService<IHookExecutor>();

        var hook = new Core.Hooks.WebHook
        {
            Url = Url,
            Method = Method,
            BodyType = BodyType,
            Body = string.IsNullOrEmpty(Body)
                ? """{"subject": "%subject%", "body": "%body%"}"""
                : Body,
            Headers = Headers,
            IgnoreSslCertificate = IgnoreSslCertificate,
            TimeoutSeconds = TimeoutSeconds,
            Auth = Auth
        };

        var variables = new Dictionary<string, string>
        {
            ["subject"] = message.Subject,
            ["body"] = message.Body
        };

        var result = await hookExecutor.ExecuteAsync(hook, variables);

        return result.Success
            ? OperationResult.Ok()
            : OperationResult.Fail(result.ErrorMessage ?? $"HTTP {result.StatusCode}");
    }
}
