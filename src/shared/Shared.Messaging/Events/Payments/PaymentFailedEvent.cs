namespace Shared.Messaging.Events.Payments;

public sealed record PaymentFailedEvent(
    Guid CorrelationId,
    int PaymentId,
    string Reason
);
