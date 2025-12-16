namespace Shared.Messaging.Events.Payments;

public sealed record PaymentCompletedEvent(
    int PaymentId,
    string MerchantId,
    decimal Amount,
    string Currency,
    string ProviderTransactionId
);
