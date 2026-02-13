/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Session;

public interface ISessionHubService
{
    Task ForceLogoutAsync(string hubConnectionId, string message);
    Task SendMessageAsync(string hubConnectionId, string message, string title, MessageSeverity severity);
    Task SendMessageToUserAsync(string userName, string message, string title, MessageSeverity severity);
    Task BroadcastMessageAsync(string message, string title, MessageSeverity severity);
    Task RefreshSessionAsync(string hubConnectionId);
}
