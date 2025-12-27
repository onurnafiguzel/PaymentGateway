using MassTransit;
using Microsoft.EntityFrameworkCore;
using PaymentOrchestrator.Application.ReadModels.Common;
using PaymentOrchestrator.Application.ReadModels.Payments.Abstractions;
using Shared.Messaging.Events.Payments;

namespace PaymentOrchestrator.Application.ReadModels.Payments.Consumers;

public sealed class PaymentFailedProjectionConsumer
    : IConsumer<PaymentFailedEvent>
{
    private readonly IPaymentReadDbContext _db;

    public PaymentFailedProjectionConsumer(IPaymentReadDbContext db)
    {
        _db = db;
    }

    public async Task Consume(ConsumeContext<PaymentFailedEvent> context)
    {
        var evt = context.Message;
        var messageId = context.MessageId ?? Guid.Empty;
        var consumerName = nameof(PaymentFailedProjectionConsumer);

        var timeline = await _db.PaymentTimelines
            .Include(x => x.Events)
            .FirstOrDefaultAsync(x => x.PaymentId == evt.PaymentId,
                context.CancellationToken);

        if (timeline is null)
            return;

        // Idempotency
        if (timeline.CurrentStatus == "Failed")
            return;

        timeline.UpdateStatus("Failed");
        timeline.AddEvent(
            "PaymentFailed",
            $"Payment failed. Reason: {evt.Reason}");

        await _db.SaveChangesAsync(context.CancellationToken);

        await ReadModelIdempotency.MarkAsProcessedAsync(
            _db, messageId, consumerName, context.CancellationToken);
    }
}
