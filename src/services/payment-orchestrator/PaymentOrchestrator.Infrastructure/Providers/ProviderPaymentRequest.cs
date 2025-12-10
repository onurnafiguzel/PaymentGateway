namespace PaymentOrchestrator.Infrastructure.Providers;

public record ProviderPaymentRequest(
    int PaymentId,
    string MerchantId,
    decimal Amount,
    string Currency
);
