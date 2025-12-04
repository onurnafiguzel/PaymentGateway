namespace Shared.Contracts.Fraud;

public record FraudCheckCompletedEvent(
    int PaymentId,
    bool IsFraud,
    string? Reason,
    DateTime CheckedAt);
