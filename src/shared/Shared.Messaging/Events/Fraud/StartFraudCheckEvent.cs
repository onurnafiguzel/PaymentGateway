namespace Shared.Messaging.Events.Fraud;

public sealed record StartFraudCheckEvent(
    Guid CorrelationId,
    int PaymentId,
    string MerchantId,
    decimal Amount,
    string Currency
);
