namespace Shared.Contracts.Fraud;

public record FraudCheckCompletedEvent(
    int PaymentId,
    string MerchantId,
    decimal Amount,
    bool IsFraud,
    string? Reason
);
