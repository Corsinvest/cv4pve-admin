/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Notifier;

public interface INotifierService
{
    IEnumerable<ModuleBase> Modules { get; }
    Task SendAsync(IEnumerable<string> notifiers, NotifierMessage message);
    IEnumerable<NotifierConfiguration> GetConfigurations(ModuleBase module);
    IEnumerable<NotifierConfiguration> GetConfigurations();
    Task SetAsync(Type setttinsType, IEnumerable<NotifierConfiguration> notifiers);
    IEnumerable<NotifierConfiguration> Get(Type setttinsType);
}
