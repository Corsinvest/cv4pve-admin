/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Commands;
using Corsinvest.ProxmoxVE.Admin.Core.Notifier;
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth;
using Microsoft.EntityFrameworkCore;

namespace Corsinvest.ProxmoxVE.Admin.Core.Extensions;

public static class ServiceScopeExtensions
{
    public static T GetRequiredService<T>(this IServiceScope scope) where T : notnull
        => scope.ServiceProvider.GetRequiredService<T>();

    public static CommandExecutor GetCommandExecutor(this IServiceScope scope) => scope.GetRequiredService<CommandExecutor>();
    public static CommandExecutor GetCommandExecutor(this IServiceProvider serviceProvider) => serviceProvider.GetRequiredService<CommandExecutor>();
    public static INotifierService GetNotifierService(this IServiceScope scope) => scope.GetRequiredService<INotifierService>();
    public static IModuleService GetModuleService(this IServiceScope scope) => scope.GetRequiredService<IModuleService>();
    public static ILoggerFactory GetLoggerFactory(this IServiceScope scope) => scope.GetRequiredService<ILoggerFactory>();
    public static IAuditService GetAuditService(this IServiceScope scope) => scope.GetRequiredService<IAuditService>();
    public static IBackgroundJobService GetBackgroundJobService(this IServiceScope scope) => scope.GetRequiredService<IBackgroundJobService>();
    public static ISettingsService GetSettingsService(this IServiceScope scope) => scope.GetRequiredService<ISettingsService>();
    public static IAdminService GetAdminService(this IServiceScope scope) => scope.GetRequiredService<IAdminService>();
    public static ClusterClient GetClusterClient(this IServiceScope scope, string clusterName) => scope.GetAdminService()[clusterName];
    public static EventNotificationService GetEventNotificationService(this IServiceScope scope) => scope.GetRequiredService<EventNotificationService>();

    public static async Task<TContext> GetDbContextAsync<TContext>(this IServiceScope scope) where TContext : DbContext
    {
        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<TContext>>();
        return await factory.CreateDbContextAsync();
    }

    public static TContext GetDbContext<TContext>(this IServiceScope scope) where TContext : DbContext
    {
        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<TContext>>();
        return factory.CreateDbContext();
    }
}
