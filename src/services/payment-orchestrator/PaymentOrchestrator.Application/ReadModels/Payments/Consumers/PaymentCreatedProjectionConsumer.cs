using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaymentOrchestrator.Application.ReadModels.Common;
using PaymentOrchestrator.Application.ReadModels.Payments.Abstractions;
using PaymentOrchestrator.Application.ReadModels.Payments.Entities;
using Shared.Messaging.Events.Payments;

namespace PaymentOrchestrator.Application.ReadModels.Payments.Consumers;

public sealed class PaymentCreatedProjectionConsumer
    : IConsumer<PaymentCreatedEvent>
{
    private readonly IPaymentReadDbContext _db;
    private readonly ILogger<PaymentCreatedProjectionConsumer> _logger;


    public PaymentCreatedProjectionConsumer(IPaymentReadDbContext db, ILogger<PaymentCreatedProjectionConsumer> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentCreatedEvent> context)
    {
        _logger.LogInformation("PaymentCreatedProjectionConsumer consumed start {PaymentId}", context.Message.PaymentId);

        var msg = context.Message;

        var messageId = context.MessageId ?? Guid.Empty;
        var consumerName = nameof(PaymentCreatedProjectionConsumer);

        // Read-model idempotency (DB-backed)
        if (await ReadModelIdempotency.IsAlreadyProcessedAsync(
                _db, messageId, consumerName, context.CancellationToken))
            return;

        // Timeline zaten varsa tekrar oluşturma
        var exists = await _db.PaymentTimelines
            .AnyAsync(x => x.PaymentId == msg.PaymentId, context.CancellationToken);

        if (!exists)
        {
            var timeline = new PaymentTimeline(msg.PaymentId);
            timeline.AddEvent(
                "PaymentCreated",
                $"Payment created. Amount: {msg.Amount} {msg.Currency}");

            _db.PaymentTimelines.Add(timeline);
            await _db.SaveChangesAsync(context.CancellationToken);
        }

        await ReadModelIdempotency.MarkAsProcessedAsync(
            _db, messageId, consumerName, context.CancellationToken);

        _logger.LogInformation("PaymentCreatedProjectionConsumer consumed end {PaymentId}", context.Message.PaymentId);

    }
}
