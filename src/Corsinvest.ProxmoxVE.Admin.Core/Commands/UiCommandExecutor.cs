/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Commands.Models;

namespace Corsinvest.ProxmoxVE.Admin.Core.Commands;

public class UiCommandExecutor(ICommandExecutor executor,
                               NotificationService notificationService,
                               IStringLocalizerFactory localizerFactory) : IUiCommandExecutor
{
    private IStringLocalizer? _localizer;
    private IStringLocalizer Localizer => _localizer ??= localizerFactory.Create(typeof(UiCommandExecutor));

    public async Task<TResult> ExecuteAndNotifyAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default)
        where TResult : CommandResult
    {
        var result = await executor.ExecuteAsync(command, cancellationToken);
        if (!result.IsSuccess)
        {
            var severity = result.IsPermissionDenied
                                ? NotificationSeverity.Warning
                                : NotificationSeverity.Error;
            var summary = result.IsPermissionDenied
                                ? Localizer["Permission denied"].Value
                                : Localizer["Operation failed"].Value;

            notificationService.Notify(severity, summary, result.ErrorMessage ?? string.Empty);
        }
        return result;
    }
}
