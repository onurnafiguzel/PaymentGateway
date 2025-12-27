using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaymentOrchestrator.Application.ReadModels.Common;
using PaymentOrchestrator.Application.ReadModels.Payments.Abstractions;
using PaymentOrchestrator.Application.ReadModels.Payments.Entities;
using Shared.Messaging.Events.Fraud;

namespace PaymentOrchestrator.Application.ReadModels.Payments.Consumers;

public sealed class FraudCheckCompletedProjectionConsumer
    : IConsumer<FraudCheckCompletedEvent>
{
    private readonly IPaymentReadDbContext _db;
    private readonly ILogger<FraudCheckCompletedProjectionConsumer> _logger;

    public FraudCheckCompletedProjectionConsumer(
        IPaymentReadDbContext db,
        ILogger<FraudCheckCompletedProjectionConsumer> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<FraudCheckCompletedEvent> context)
    {
        var evt = context.Message;

        _logger.LogInformation(
            "FraudCheckCompletedProjectionConsumer start | PaymentId={PaymentId}",
            evt.PaymentId);

        var messageId = context.MessageId ?? Guid.Empty;
        var consumerName = nameof(FraudCheckCompletedProjectionConsumer);

        // Idempotency
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

        // Event INSERT (TRACKING YOK)
        _db.PaymentTimelineEvents.Add(
            new PaymentTimelineEvent(
                evt.PaymentId,
                "FraudCheckCompleted",
                evt.IsFraud
                    ? $"Fraud detected: {evt.Reason}"
                    : "Fraud check passed"));

        await _db.SaveChangesAsync(context.CancellationToken);

        // Idempotency mark
        await ReadModelIdempotency.MarkAsProcessedAsync(
            _db, messageId, consumerName, context.CancellationToken);

        _logger.LogInformation(
            "FraudCheckCompletedProjectionConsumer end | PaymentId={PaymentId}",
            evt.PaymentId);
    }
}
