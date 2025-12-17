namespace Shared.Messaging.Events.Payments;

public record FraudTimeoutExpiredEvent
{
    public Guid CorrelationId { get; init; }
}
