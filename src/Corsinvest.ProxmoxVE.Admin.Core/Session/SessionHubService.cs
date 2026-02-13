/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Session;

internal class SessionHubService : ISessionHubService
{
    public Task ForceLogoutAsync(string hubConnectionId, string? message = null)
        => Task.CompletedTask;

    public Task SendMessageAsync(string hubConnectionId, string message, string? title = null, MessageSeverity severity = MessageSeverity.Info)
        => Task.CompletedTask;

    public Task SendMessageToUserAsync(string userName, string message, string? title = null, MessageSeverity severity = MessageSeverity.Info)
        => Task.CompletedTask;

    public Task BroadcastMessageAsync(string message, string? title = null, MessageSeverity severity = MessageSeverity.Info)
        => Task.CompletedTask;

    public Task RefreshSessionAsync(string hubConnectionId)
        => Task.CompletedTask;
}
