/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Hooks;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHookExecutor(this IServiceCollection services)
        => services.AddTransient<IHookExecutor, HookExecutor>();
}
