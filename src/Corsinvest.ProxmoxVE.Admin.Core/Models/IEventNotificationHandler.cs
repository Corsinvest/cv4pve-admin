namespace Corsinvest.ProxmoxVE.Admin.Core.Models;

public interface IEventNotificationHandler<in T> where T : IEventNotification
{
    Task HandleAsync(T notification, CancellationToken cancellationToken = default);
}
