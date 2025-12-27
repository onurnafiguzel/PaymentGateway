using MassTransit;
using Microsoft.EntityFrameworkCore;
using PaymentOrchestrator.Application.ReadModels.Common;
using PaymentOrchestrator.Application.ReadModels.Payments.Abstractions;
using PaymentOrchestrator.Application.ReadModels.Payments.Entities;
using Shared.Messaging.Events.Payments;

namespace PaymentOrchestrator.Application.ReadModels.Payments.Consumers;

public sealed class PaymentCreatedProjectionConsumer
    : IConsumer<PaymentCreatedEvent>
{
    private readonly IPaymentReadDbContext _db;

    public PaymentCreatedProjectionConsumer(IPaymentReadDbContext db)
    {
        _db = db;
    }

    public async Task Consume(ConsumeContext<PaymentCreatedEvent> context)
    {
        var msg = context.Message;
        var messageId = context.MessageId ?? Guid.Empty;
        var consumerName = nameof(PaymentCreatedProjectionConsumer);

        if (await _db.PaymentTimelines
            .AnyAsync(x => x.PaymentId == msg.PaymentId))
            return; // idempotent

        var timeline = new PaymentTimeline(msg.PaymentId);
        timeline.AddEvent(
            "PaymentCreated",
            $"Payment created. Amount: {msg.Amount} {msg.Currency}");

        _db.PaymentTimelines.Add(timeline);
        await _db.SaveChangesAsync(context.CancellationToken);

        await ReadModelIdempotency.MarkAsProcessedAsync(
            _db, messageId, consumerName, context.CancellationToken);
    }
}
