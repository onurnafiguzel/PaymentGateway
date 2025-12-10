namespace Shared.Contracts.Payments;

public record ProviderInitiationRequestedEvent(
    int PaymentId,
    string MerchantId,
    decimal Amount,
    string Currency,
    string ProviderName
);
