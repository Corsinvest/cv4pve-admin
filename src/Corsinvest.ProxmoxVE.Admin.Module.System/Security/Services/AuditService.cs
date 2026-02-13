/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth;

namespace Corsinvest.ProxmoxVE.Admin.Module.System.Security.Services;

public class AuditService(ILogger<AuditService> logger, IHttpContextAccessor httpContextAccessor) : IAuditService
{
    public Type Render { get; } = typeof(Core.Components.SubscriptionRequired);

    public Task LogAsync(string userName, string action, bool success, string? details = null)
    {
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

    public Task LogAsync(string action, bool success, string? details = null)
        => LogAsync(httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System", action, success, details);
}
