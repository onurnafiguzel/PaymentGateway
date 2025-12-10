namespace Shared.Contracts.Events.Fraud;

public record FraudCheckRequestedEvent(
    int PaymentId,
    string MerchantId,
    decimal Amount
);


public record FraudCheckCompletedEvent(
    int PaymentId,
    string MerchantId,
    decimal Amount,
    bool IsFraud,
    string? Reason
);
