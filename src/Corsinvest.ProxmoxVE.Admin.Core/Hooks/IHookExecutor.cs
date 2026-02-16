/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Hooks;

public interface IHookExecutor
{
    /// <summary>
    /// Execute a webhook replacing %key% placeholders in Url, Headers and Body.
    /// </summary>
    Task<WebHookResult> ExecuteAsync(WebHook hook, IReadOnlyDictionary<string, string> variables);
}
