namespace Shared.Contracts.Providers;

public record ProviderPaymentRequest(
    int PaymentId,
    string MerchantId,
    decimal Amount,
    string Currency,
    string ProviderName);
