namespace Shared.Contracts.Payments;

public record PaymentFailedEvent(
    int PaymentId,
    string Reason,
    DateTime FailedAt);
