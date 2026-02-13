/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Models;

public interface IEventNotificationHandler<in T> where T : IEventNotification
{
    Task HandleAsync(T notification, CancellationToken cancellationToken = default);
}
