/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth;

namespace Corsinvest.ProxmoxVE.Admin.Module.System.Security.Services;

public class AuditService(ILogger<AuditService> logger, ICurrentUserService currentUserService) : IAuditService
{
    public Type Render { get; } = typeof(Core.Components.SubscriptionRequired);

    public Task LogAsync(string action, bool success, string? details = null)
    {
        var userName = currentUserService.UserName;
        if (success)
        {
            logger.LogInformation("Successful {Action} for user: {UserName}. Details: {Details}", action, userName, details);
        }
        else
        {
            logger.LogWarning("Failed {Action} for user: {UserName}. Details: {Details}", action, userName, details);
        }

        return Task.CompletedTask;
    }
}
