namespace Shared.Messaging.Events.Payments;

public sealed record ReplayPaymentRequestedEvent(
    Guid CorrelationId,
    Guid PaymentId,
    string Reason);
