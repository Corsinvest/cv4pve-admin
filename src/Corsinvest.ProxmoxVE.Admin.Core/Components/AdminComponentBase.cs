/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth.Permissions;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components;

public abstract class AdminComponentBase : ComponentBase
{
    [Inject] private IStringLocalizerFactory LocalizerFactory { get; set; } = default!;
    [Inject] private ILoggerFactory LoggerFactory { get; set; } = default!;
    [Inject] protected IPermissionService PermissionService { get; set; } = default!;

    protected ILogger Logger => field ??= LoggerFactory.CreateLogger(GetType());

    protected IStringLocalizer L => field ??= LocalizerFactory.Create(GetType());

    protected static string UniqueID
        => Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Replace("/", "-")
                  .Replace("+", "-")[..10];

    protected async Task<bool> HasPermissionAsync(string clusterName, Permission permission)
        => await PermissionService.HasAsync(clusterName, permission);

    protected async Task<bool> HasPermissionAsync(Permission permission)
        => await PermissionService.HasAsync(ApplicationHelper.AllClusterName, permission);
}
