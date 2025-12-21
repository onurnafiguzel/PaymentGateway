namespace Shared.Messaging.Events.Payments;

public sealed record PaymentFailedEvent(
    Guid CorrelationId,
    Guid PaymentId,
    string Reason
);
