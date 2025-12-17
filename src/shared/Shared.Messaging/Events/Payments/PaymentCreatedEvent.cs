using System.ComponentModel;

namespace Shared.Messaging.Events.Payments;

public record PaymentCreatedEvent
{
    public Guid CorrelationId { get; init; }
    public int PaymentId { get; init; }
    public string MerchantId { get; init; } = default!;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = default!;
    public DateTime CreatedAt { get; init; }
}
