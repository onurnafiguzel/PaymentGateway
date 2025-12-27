using Microsoft.EntityFrameworkCore;
using PaymentOrchestrator.Application.ReadModels.Common.Entities;
using PaymentOrchestrator.Application.ReadModels.Payments.Abstractions;

namespace PaymentOrchestrator.Application.ReadModels.Common;

public static class ReadModelIdempotency
{
    public static async Task<bool> IsAlreadyProcessedAsync(
        IPaymentReadDbContext db,
        Guid messageId,
        string consumerName,
        CancellationToken ct)
    {
        return await db.Set<ProcessedReadEvent>()
            .AnyAsync(x =>
                x.MessageId == messageId &&
                x.ConsumerName == consumerName,
                ct);
    }

    public static async Task MarkAsProcessedAsync(
        IPaymentReadDbContext db,
        Guid messageId,
        string consumerName,
        CancellationToken ct)
    {
        db.Set<ProcessedReadEvent>().Add(
            new ProcessedReadEvent(messageId, consumerName));

        await db.SaveChangesAsync(ct);
    }
}
