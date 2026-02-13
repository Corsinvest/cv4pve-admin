/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Helpers;

public abstract class BaseActionHelper<TModule, TSettings, TDataChangedNotification>
    where TModule : ModuleBase
    where TSettings : class, new()
    where TDataChangedNotification : EventArgsNotification, new()
{
    protected static TSettings GetModuleSettings(IServiceScope scope, string clusterName)
        => scope.GetSettingsService().GetForModule<TModule, TSettings>(clusterName);

    protected static async Task PublishDataChangedAsync(IServiceScope scope)
        => await scope.GetEventNotificationService().PublishAsync(new TDataChangedNotification());
}
