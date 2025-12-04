namespace Shared.Messaging.Events.Payments;

public sealed record PaymentCreatedEvent(
    int PaymentId,
    string MerchantId,
    decimal Amount,
    string Currency,
    DateTime CreatedAt
);
