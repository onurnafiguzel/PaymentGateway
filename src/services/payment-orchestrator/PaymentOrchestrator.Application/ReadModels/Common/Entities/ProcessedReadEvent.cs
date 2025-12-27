namespace PaymentOrchestrator.Application.ReadModels.Common.Entities;

public sealed class ProcessedReadEvent
{
    private ProcessedReadEvent() { } // EF

    public ProcessedReadEvent(
        Guid messageId,
        string consumerName)
    {
        Id = Guid.NewGuid();
        MessageId = messageId;
        ConsumerName = consumerName;
        ProcessedAtUtc = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid MessageId { get; private set; }
    public string ConsumerName { get; private set; } = default!;
    public DateTime ProcessedAtUtc { get; private set; }
}
