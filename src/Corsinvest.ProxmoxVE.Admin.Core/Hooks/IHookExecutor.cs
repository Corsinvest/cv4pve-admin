/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Hooks;

public interface IHookExecutor
{
    /// <summary>
    /// Execute a webhook replacing %key% placeholders in Url, Headers and Body.
    /// Pass raw values: each one is escaped for its destination (URL-encoded in the Url,
    /// JSON or XML escaped in the Body according to BodyType, line breaks removed in Headers).
    /// </summary>
    Task<WebHookResult> ExecuteAsync(WebHook hook, IReadOnlyDictionary<string, string> variables);
}
