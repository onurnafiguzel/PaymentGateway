namespace Shared.Messaging.Events.Payments;

public sealed record ReplayPaymentRequestedEvent(
    Guid PaymentId,
    string Reason
);
