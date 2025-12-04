namespace Shared.Contracts.Payments;

public record PaymentCompletedEvent(
    int PaymentId,
    string ProviderTransactionId,
    DateTime CompletedAt);
