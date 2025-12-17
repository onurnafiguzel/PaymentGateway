namespace Shared.Messaging.Events.Payments;

public sealed record ReplayPaymentRequestedEvent(
    int PaymentId,
    string Reason
);
