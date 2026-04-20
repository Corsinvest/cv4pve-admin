/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Commands.Models;

namespace Corsinvest.ProxmoxVE.Admin.Core.Commands;

public interface IUiCommandExecutor
{
    Task<TResult> ExecuteAndNotifyAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default)
        where TResult : CommandResult;
}
