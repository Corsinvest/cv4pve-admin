/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Hooks;

public class WebHook
{
    public string Url { get; set; } = string.Empty;
    public WebHookHttpMethod Method { get; set; } = WebHookHttpMethod.Post;
    public Dictionary<string, string> Headers { get; set; } = [];
    public string Body { get; set; } = string.Empty;
    public WebHookBodyType BodyType { get; set; } = WebHookBodyType.Json;
    public bool IgnoreSslCertificate { get; set; }
    public int TimeoutSeconds { get; set; } = 30;
    public WebHookAuth Auth { get; set; } = new();
}
