namespace Shared.Messaging.Events.Fraud;

public sealed record FraudCheckCompletedEvent(
    int PaymentId,
    bool IsFraud,
    string? Reason,
    DateTime CheckedAt
);
