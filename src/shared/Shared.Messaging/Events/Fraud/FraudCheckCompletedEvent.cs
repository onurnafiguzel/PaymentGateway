namespace Shared.Messaging.Events.Fraud;

public sealed record FraudCheckCompletedEvent(
    Guid CorrelationId,
    int PaymentId,
    bool IsFraud,
    string? Reason,
    DateTime CheckedAt
);
