namespace Shared.Messaging.Events.Fraud;

public sealed record StartFraudCheckEvent(
    Guid CorrelationId,
    Guid PaymentId,
    string MerchantId,
    decimal Amount,
    string Currency
);
