using Shared.Messaging.Events.Common;
using System.Diagnostics;

namespace PaymentOrchestrator.Infrastructure.Messaging;

public sealed class InMemoryEventBus : IEventBus
{
    private readonly Dictionary<Type, List<Func<object, Task>>> _handlers = new();
    private static readonly ActivitySource ActivitySource = new("PaymentOrchestrator");

    public Task SubscribeAsync<T>(Func<T, Task> handler)
    {
        var type = typeof(T);

        if (!_handlers.ContainsKey(type))
            _handlers[type] = new List<Func<object, Task>>();

        // Consumer Activity Creation
        _handlers[type].Add(async evt =>
        {
            using var activity = ActivitySource.StartActivity(
                $"event.handle:{type.Name}",
                ActivityKind.Consumer
            );

            activity?.SetTag("event.name", type.Name);
            activity?.SetTag("event.type", type.FullName);
            activity?.SetTag("handler.name", handler.Method.Name);

            await handler((T)evt);
        });

        return Task.CompletedTask;
    }

    public async Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default)
    {
        var type = typeof(T);

        // Producer Activity Creation
        using var activity = ActivitySource.StartActivity(
            $"event.publish:{type.Name}",
            ActivityKind.Producer
        );

        activity?.SetTag("event.name", type.Name);
        activity?.SetTag("event.type", type.FullName);
        activity?.SetTag("event.assembly", type.Assembly.GetName().Name);
        activity?.SetTag("event.payload", @event!.ToString());

        if (!_handlers.TryGetValue(type, out var handlers))
            return;

        foreach (var handler in handlers)
            await handler(@event);
    }
}
