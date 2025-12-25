using Shared.Messaging.Events.Payments;

namespace PaymentOrchestrator.Application.ReadModels.Payments.Entities;

public sealed class PaymentTimeline
{
    private PaymentTimeline() { } // EF

    public PaymentTimeline(Guid paymentId)
    {
        PaymentId = paymentId;
        CreatedAtUtc = DateTime.UtcNow;
        Events = new List<PaymentTimelineEvent>();
    }

    public Guid PaymentId { get; private set; }

    public string CurrentStatus { get; private set; } = "Pending";

    public DateTime CreatedAtUtc { get; private set; }

    public List<PaymentTimelineEvent> Events { get; private set; }

    public void UpdateStatus(string status)
        => CurrentStatus = status;

    public void AddEvent(string type, string description)
    {
        Events.Add(new PaymentTimelineEvent(
            PaymentId,
            type,
            description
        ));
    }
}
