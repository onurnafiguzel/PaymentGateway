namespace PaymentOrchestrator.Application.ReadModels.Payments.Entities;

public sealed class PaymentTimelineEvent
{
    private PaymentTimelineEvent() { } // EF

    public PaymentTimelineEvent(
        Guid paymentId,
        string type,
        string description)
    {
        Id = Guid.NewGuid();
        PaymentId = paymentId;
        Type = type;
        Description = description;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid PaymentId { get; private set; }
    public string Type { get; private set; }
    public string Description { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
}
