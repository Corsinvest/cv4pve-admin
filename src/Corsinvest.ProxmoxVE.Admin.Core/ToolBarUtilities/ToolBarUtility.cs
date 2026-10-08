/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth;

namespace Corsinvest.ProxmoxVE.Admin.Core.ToolBarUtilities;

/// <summary>
/// Checks the permission and writes the audit entry around the action, so a utility only says
/// which permission it needs and what it does.
/// </summary>
public abstract class ToolBarUtility<T>(IAuditService auditService)
{
    public abstract string Icon { get; }
    public abstract string Text { get; }
    public virtual bool RequireConfirm => true;
    public virtual bool IsVIsible(T item) => true;

    protected abstract string AuditAction { get; }
    protected abstract string GetAuditContext(string clusterName, T item);

    public abstract Task<bool> HasPermissionAsync(string clusterName, T item);

    /// <summary>Runs the action and returns whether it succeeded.</summary>
    protected abstract Task<bool> ExecuteCoreAsync(string clusterName, T item);

    public async Task ExecuteAsync(string clusterName, T item)
    {
        var context = GetAuditContext(clusterName, item);

        if (!await HasPermissionAsync(clusterName, item))
        {
            await auditService.LogAsync(AuditAction, false, $"Permission denied. {context}");
            return;
        }

        try
        {
            var success = await ExecuteCoreAsync(clusterName, item);
            await auditService.LogAsync(AuditAction, success, context);
        }
        catch (Exception ex)
        {
            await auditService.LogAsync(AuditAction, false, $"Exception: {ex.Message}. {context}");
            throw;
        }
    }
}
