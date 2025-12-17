namespace Shared.Messaging.Events.Payments;

public sealed record PaymentCompletedEvent(
    Guid CorrelationId,
    int PaymentId,
    string MerchantId,
    decimal Amount,
    string Currency,
    string ProviderTransactionId
);
