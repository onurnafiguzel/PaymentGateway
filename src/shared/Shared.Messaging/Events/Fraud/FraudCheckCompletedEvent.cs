namespace Shared.Messaging.Events.Fraud;

public sealed record FraudCheckCompletedEvent(
    Guid CorrelationId,
    Guid PaymentId,
    bool IsFraud,
    string? Reason,
    DateTime CheckedAt
);
