namespace Shared.Messaging.Events.Payments;

public sealed record ProviderInitiationRequestedEvent(
    Guid CorrelationId,
    int PaymentId,
    string MerchantId,
    decimal Amount,
    string Currency,
    string ProviderName
);
