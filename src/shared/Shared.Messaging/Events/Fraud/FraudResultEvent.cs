namespace Shared.Messaging.Events.Fraud;

public sealed record FraudResultEvent(
    int PaymentId,
    bool IsFraud,
    DateTime CheckedAt
);
