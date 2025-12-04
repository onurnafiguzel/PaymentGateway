namespace PaymentOrchestrator.Api.Contracts.Payments;

public sealed class CreatePaymentRequest
{
    public string MerchantId { get; set; } = default!;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "TRY";
}
