namespace PaymentOrchestrator.Domain.Payments;

public sealed class PaymentReplayHistory
{
    public Guid Id { get; private set; }
    public Guid PaymentId { get; private set; }
    public string Reason { get; private set; } = default!;
    public string TriggeredBy { get; private set; } = default!;
    public DateTime CreatedAtUtc { get; private set; }

    private PaymentReplayHistory() { } // EF

    public PaymentReplayHistory(
        Guid paymentId,
        string reason,
        string triggeredBy)
    {
        Id = Guid.NewGuid();
        PaymentId = paymentId;
        Reason = reason;
        TriggeredBy = triggeredBy;
        CreatedAtUtc = DateTime.UtcNow;
    }

}
