using PaymentOrchestrator.Application.Common.Events;

namespace PaymentOrchestrator.Infrastructure.Messaging;

public sealed class InMemoryEventBus : IEventBus
{
    private readonly Dictionary<Type, List<Func<object, Task>>> _handlers = new();

    public Task SubscribeAsync<T>(Func<T, Task> handler)
    {
        var type = typeof(T);

        if (!_handlers.ContainsKey(type))
            _handlers[type] = new List<Func<object, Task>>();

        _handlers[type].Add(evt => handler((T)evt));

        return Task.CompletedTask;
    }

    public async Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default)
    {
        var type = typeof(T);

        if (!_handlers.TryGetValue(type, out var handlers))
            return;

        foreach (var handler in handlers)
            await handler(@event);
    }
}
