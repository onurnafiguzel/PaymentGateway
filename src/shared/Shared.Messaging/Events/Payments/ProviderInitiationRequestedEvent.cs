namespace Shared.Messaging.Events.Payments;

public sealed record ProviderInitiationRequestedEvent(
    int PaymentId,
    string MerchantId,
    decimal Amount,
    string Currency,
    string ProviderName
);
