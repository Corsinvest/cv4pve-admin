/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Hooks;

public class WebHookAuth
{
    public WebHookAuthType Type { get; set; } = WebHookAuthType.None;

    // Basic
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    // Bearer
    public string Token { get; set; } = string.Empty;

    // ApiKey
    public string ApiKeyHeader { get; set; } = string.Empty;
    public string ApiKeyValue { get; set; } = string.Empty;
}
