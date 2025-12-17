namespace Shared.Messaging.Events.Payments;

public sealed record ProviderTimeoutExpiredEvent
{
    public Guid CorrelationId { get; init; }
}
