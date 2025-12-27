using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaymentOrchestrator.Application.ReadModels.Common;
using PaymentOrchestrator.Application.ReadModels.Payments.Abstractions;
using PaymentOrchestrator.Application.ReadModels.Payments.Entities;
using Shared.Messaging.Events.Payments;

namespace PaymentOrchestrator.Application.ReadModels.Payments.Consumers;

public sealed class PaymentFailedProjectionConsumer
    : IConsumer<PaymentFailedEvent>
{
    private readonly IPaymentReadDbContext _db;
    private readonly ILogger<PaymentFailedProjectionConsumer> _logger;

    public PaymentFailedProjectionConsumer(
        IPaymentReadDbContext db,
        ILogger<PaymentFailedProjectionConsumer> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentFailedEvent> context)
    {
        var evt = context.Message;

        _logger.LogInformation(
            "PaymentFailedProjectionConsumer start | PaymentId={PaymentId}",
            evt.PaymentId);

        var messageId = context.MessageId ?? Guid.Empty;
        var consumerName = nameof(PaymentFailedProjectionConsumer);

        if (await ReadModelIdempotency.IsAlreadyProcessedAsync(
                _db, messageId, consumerName, context.CancellationToken))
            return;

        // Timeline var mı?
        var exists = await _db.PaymentTimelines
            .AnyAsync(x => x.PaymentId == evt.PaymentId, context.CancellationToken);

        if (!exists)
        {
            _db.PaymentTimelines.Add(new PaymentTimeline(evt.PaymentId));
            await _db.SaveChangesAsync(context.CancellationToken);
        }

        // Status UPDATE (tracking yok)
        await _db.PaymentTimelines
            .Where(x => x.PaymentId == evt.PaymentId)
            .ExecuteUpdateAsync(
                s => s.SetProperty(x => x.CurrentStatus, "Failed"),
                context.CancellationToken);

        // Event INSERT
        _db.PaymentTimelineEvents.Add(
            new PaymentTimelineEvent(
                evt.PaymentId,
                "PaymentFailed",
                $"Payment failed. Reason: {evt.Reason}"));

        await _db.SaveChangesAsync(context.CancellationToken);

        await ReadModelIdempotency.MarkAsProcessedAsync(
            _db, messageId, consumerName, context.CancellationToken);

        _logger.LogInformation(
            "PaymentFailedProjectionConsumer end | PaymentId={PaymentId}",
            evt.PaymentId);
    }
}
