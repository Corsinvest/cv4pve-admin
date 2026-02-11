using System.Reflection;

namespace Corsinvest.ProxmoxVE.Admin.Core.Commands;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCommands(this IServiceCollection services, params Assembly[] assemblies)
    {
        // Register CommandExecutor
        services.AddScoped<CommandExecutor>();

        // Auto-register all ICommandHandler implementations
        var assembliesToScan = assemblies.Length > 0
            ? assemblies
            : [Assembly.GetCallingAssembly()];

        foreach (var assembly in assembliesToScan)
        {
            var handlerTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract)
                .Select(t => new
                {
                    ImplementationType = t,
                    HandlerInterface = t.GetInterfaces()
                        .FirstOrDefault(i => i.IsGenericType &&
                                           i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>))
                })
                .Where(x => x.HandlerInterface != null);

            foreach (var handler in handlerTypes)
            {
                services.AddScoped(handler.HandlerInterface!, handler.ImplementationType);
            }
        }

        return services;
    }

    public static IServiceCollection AddCommandExecutor(this IServiceCollection services)
        => services.AddScoped<CommandExecutor>();

    public static IServiceCollection AddCommandHandler<THandler>(this IServiceCollection services)
        where THandler : class
    {
        var handlerInterface = typeof(THandler).GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType &&
                               i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>))
            ?? throw new InvalidOperationException(
                $"Type {typeof(THandler).Name} does not implement ICommandHandler<,>");

        services.AddScoped(handlerInterface, typeof(THandler));
        return services;
    }
}
