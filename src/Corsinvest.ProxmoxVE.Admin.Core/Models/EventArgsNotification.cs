namespace Corsinvest.ProxmoxVE.Admin.Core.Models;

public delegate Task EventNotificationHandlerDelegate<in T>(T notification, CancellationToken cancellationToken = default) where T : IEventNotification;

public abstract class EventArgsNotification : IEventNotification;

public abstract class EventArgsNotification<T> : IEventNotification
{
    public T Item { get; set; } = default!;
}
