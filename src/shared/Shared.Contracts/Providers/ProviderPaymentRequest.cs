namespace Shared.Contracts.Providers;

public record ProviderPaymentRequest(
    Guid PaymentId,
    string MerchantId,
    decimal Amount,
    string Currency,
    string ProviderName);
