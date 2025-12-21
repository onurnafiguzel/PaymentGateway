namespace Shared.Messaging.Events.Payments;

public sealed record PaymentCompletedEvent(
    Guid CorrelationId,
    Guid PaymentId,
    string MerchantId,
    decimal Amount,
    string Currency
);
