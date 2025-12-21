namespace Shared.Messaging.Events.Payments;

public sealed record ProviderInitiationRequestedEvent(
    Guid CorrelationId,
    Guid PaymentId,
    string MerchantId,
    decimal Amount,
    string Currency,
    string ProviderName
);
