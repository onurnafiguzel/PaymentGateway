namespace PaymentOrchestrator.Application.Common.Events;

public interface IEventBus
{
    Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default);
    Task SubscribeAsync<T>(Func<T, Task> handler);
}
