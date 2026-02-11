using System.Collections.Concurrent;
using System.Reflection;

namespace Corsinvest.ProxmoxVE.Admin.Core.Services;

public class EventNotificationService(IServiceProvider serviceProvider, ILogger<EventNotificationService> logger)
{
    private readonly ConcurrentDictionary<Type, HashSet<Func<IEventNotification, CancellationToken, Task>>> _handlers = new();
    private readonly ConcurrentDictionary<Delegate, Func<IEventNotification, CancellationToken, Task>> _wrappers = new();
    private readonly Lock _lock = new();

    public IDisposable Subscribe<T>(Func<T, Task> handler) where T : IEventNotification
    {
        async Task Wrapper(IEventNotification notification, CancellationToken cancellationToken)
        {
            if (notification is T typedNotification) { await handler(typedNotification); }
        }

        _wrappers[handler] = Wrapper;

        lock (_lock)
        {
            var handlers = _handlers.GetOrAdd(typeof(T), _ => []);
            handlers.Add(Wrapper);
        }

        return new Subscription(() => Unsubscribe(handler));
    }

    public void Unsubscribe<T>(Func<T, Task> handler) where T : IEventNotification
    {
        if (!_wrappers.TryRemove(handler, out var wrapper)) { return; }

        lock (_lock)
        {
            if (_handlers.TryGetValue(typeof(T), out var handlers))
            {
                handlers.Remove(wrapper);
                if (handlers.Count == 0) { _handlers.TryRemove(typeof(T), out _); }
            }
        }
    }

    public async Task PublishAsync<T>(T notification, bool sequential = false, CancellationToken cancellationToken = default) where T : IEventNotification
    {
        var tasks = new List<Task>();

        // Subscribed handlers
        Func<IEventNotification, CancellationToken, Task>[] handlers;
        lock (_lock)
        {
            handlers = _handlers.TryGetValue(typeof(T), out var h)
                        ? [.. h]
                        : [];
        }

        foreach (var handler in handlers)
        {
            if (sequential) { await InvokeHandlerSafe(handler, notification, cancellationToken); }
            else { tasks.Add(InvokeHandlerSafe(handler, notification, cancellationToken)); }
        }

        // DI handlers
        foreach (var handler in serviceProvider.GetServices<IEventNotificationHandler<T>>())
        {
            if (sequential) { await InvokeHandlerSafe(async (n, ct) => await handler.HandleAsync((T)n, ct), notification, cancellationToken); }
            else { tasks.Add(InvokeHandlerSafe(async (n, ct) => await handler.HandleAsync((T)n, ct), notification, cancellationToken)); }
        }

        if (!sequential && tasks.Count > 0) { await Task.WhenAll(tasks); }
    }

    public void Publish<T>(T notification, CancellationToken cancellationToken = default) where T : IEventNotification
    {
        // Subscribed handlers
        Func<IEventNotification, CancellationToken, Task>[] handlers;
        lock (_lock)
        {
            handlers = _handlers.TryGetValue(typeof(T), out var h) ? [.. h] : [];
        }

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => InvokeHandlerSafe(handler, notification, cancellationToken), cancellationToken);
        }

        // DI handlers
        foreach (var handler in serviceProvider.GetServices<IEventNotificationHandler<T>>())
        {
            var handlerCopy = handler;
            _ = Task.Run(() => InvokeHandlerSafe(async (n, ct) => await handlerCopy.HandleAsync((T)n, ct), notification, cancellationToken), cancellationToken);
        }
    }

    private async Task InvokeHandlerSafe(Func<IEventNotification, CancellationToken, Task> handler, IEventNotification notification, CancellationToken cancellationToken)
    {
        try
        {
            await handler(notification, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error executing handler for {NotificationType}", notification.GetType().Name);
        }
    }

    public static IServiceCollection RegisterHandlersFromAssembly(IServiceCollection services, Assembly assembly)
    {
        var handlerTypes = assembly.GetTypes()
                                   .Where(t => !t.IsAbstract && !t.IsInterface
                                                && t.GetInterfaces()
                                                    .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventNotificationHandler<>)))
                                   .ToList();

        foreach (var handlerType in handlerTypes)
        {
            var interfaces = handlerType.GetInterfaces()
                                        .Where(i => i.IsGenericType
                                                    && i.GetGenericTypeDefinition() == typeof(IEventNotificationHandler<>));

            foreach (var @interface in interfaces)
            {
                services.AddTransient(@interface, handlerType);
            }
        }

        return services;
    }

    public void ClearAllHandlers()
    {
        lock (_lock)
        {
            _handlers.Clear();
            _wrappers.Clear();
        }
    }

    public void ClearHandlers<T>() where T : IEventNotification
    {
        lock (_lock)
        {
            if (_handlers.TryRemove(typeof(T), out var handlers))
            {
                // Remove wrappers for this type
                var wrappersToRemove = _wrappers.Where(kv => handlers.Contains(kv.Value)).Select(kv => kv.Key).ToList();
                foreach (var key in wrappersToRemove) { _wrappers.TryRemove(key, out _); }
            }
        }
    }

    private sealed class Subscription(Action unsubscribe) : IDisposable
    {
        private Action? _unsubscribe = unsubscribe;

        public void Dispose()
        {
            _unsubscribe?.Invoke();
            _unsubscribe = null;
        }
    }
}
