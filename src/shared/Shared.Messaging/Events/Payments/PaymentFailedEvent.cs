namespace Shared.Messaging.Events.Payments;

public sealed record PaymentFailedEvent(
    int PaymentId,
    string Reason
);
