//using MediatR;

namespace Corsinvest.ProxmoxVE.Admin.Core.Services;

//public class EventNotificationService(IMediator mediator)
//{
//    private readonly List<Delegate> _handlers = [];

//    public void Subscribe<T>(Func<T, Task> handler) where T : INotification => _handlers.Add(handler);
//    public void Unsubscribe<T>(Func<T, Task> handler) where T : INotification => _handlers.Remove(handler);

//    public Task PublishAsync<T>(T eventMessage) where T : INotification
//    {
//        mediator.Publish(eventMessage);

//        foreach (var handler in _handlers.OfType<Func<T, Task>>())
//        {
//            handler(eventMessage);
//        }

//        return Task.CompletedTask;
//    }
//}

public enum PublishMode
{
    Sequential,
    ParallelWait,
    ParallelNoWait
}
