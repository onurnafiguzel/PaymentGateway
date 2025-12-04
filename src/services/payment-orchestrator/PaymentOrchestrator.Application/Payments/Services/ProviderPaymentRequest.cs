namespace PaymentOrchestrator.Application.Payments.Services;

public record ProviderPaymentRequest(
    int PaymentId,
    string MerchantId,
    decimal Amount,
    string Currency,
    string ProviderName);
