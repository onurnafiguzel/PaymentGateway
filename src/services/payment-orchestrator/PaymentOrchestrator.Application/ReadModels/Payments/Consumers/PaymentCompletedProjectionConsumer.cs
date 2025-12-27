using MassTransit;
using Microsoft.EntityFrameworkCore;
using PaymentOrchestrator.Application.ReadModels.Common;
using PaymentOrchestrator.Application.ReadModels.Payments.Abstractions;
using Shared.Messaging.Events.Payments;

namespace PaymentOrchestrator.Application.ReadModels.Payments.Consumers;

public sealed class PaymentCompletedProjectionConsumer
    : IConsumer<PaymentCompletedEvent>
{
    private readonly IPaymentReadDbContext _db;

    public PaymentCompletedProjectionConsumer(IPaymentReadDbContext db)
    {
        _db = db;
    }

    public async Task Consume(ConsumeContext<PaymentCompletedEvent> context)
    {
        var messageId = context.MessageId ?? Guid.Empty;
        var consumerName = nameof(PaymentCompletedProjectionConsumer);


        if (await ReadModelIdempotency.IsAlreadyProcessedAsync(
          _db, messageId, consumerName, context.CancellationToken))
            return;

        var timeline = await _db.PaymentTimelines
            .Include(x => x.Events)
            .FirstOrDefaultAsync(x => x.PaymentId == context.Message.PaymentId,
                context.CancellationToken);

        if (timeline is null)
            return;

        // Idempotency: aynı event iki kez gelirse tekrar yazma
        if (timeline.CurrentStatus == "Succeeded")
            return;

        timeline.UpdateStatus("Succeeded");
        timeline.AddEvent(
            "PaymentCompleted",
            $"Payment completed. PaymentId: {context.Message.PaymentId}");

        await _db.SaveChangesAsync(context.CancellationToken);

        await ReadModelIdempotency.MarkAsProcessedAsync(
            _db, messageId, consumerName, context.CancellationToken);

    }
}
