namespace Shared.Messaging.Events.Fraud;

public sealed record StartFraudCheckEvent(
    int PaymentId,
    string MerchantId,
    decimal Amount,
    string Currency
);
