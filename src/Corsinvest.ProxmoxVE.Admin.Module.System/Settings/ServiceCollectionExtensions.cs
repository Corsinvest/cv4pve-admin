/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Module.System.Settings.Services;

namespace Corsinvest.ProxmoxVE.Admin.Module.System.Settings;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSettingsAdmin(this IServiceCollection services)
        => services.AddScoped<ISettingsService, SettingsService>();
}
