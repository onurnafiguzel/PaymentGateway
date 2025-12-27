using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaymentOrchestrator.Application.ReadModels.Common;
using PaymentOrchestrator.Application.ReadModels.Payments.Abstractions;
using PaymentOrchestrator.Application.ReadModels.Payments.Entities;
using Shared.Messaging.Events.Payments;

namespace PaymentOrchestrator.Application.ReadModels.Payments.Consumers;

public sealed class PaymentCompletedProjectionConsumer
    : IConsumer<PaymentCompletedEvent>
{
    private readonly IPaymentReadDbContext _db;
    private readonly ILogger<PaymentCompletedProjectionConsumer> _logger;


    public PaymentCompletedProjectionConsumer(IPaymentReadDbContext db, ILogger<PaymentCompletedProjectionConsumer> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentCompletedEvent> context)
    {
        var evt = context.Message;

        var messageId = context.MessageId ?? Guid.Empty;
        var consumerName = nameof(PaymentCompletedProjectionConsumer);

        if (await ReadModelIdempotency.IsAlreadyProcessedAsync(
                _db, messageId, consumerName, context.CancellationToken))
            return;

        // 1️⃣ Timeline var mı?
        var exists = await _db.PaymentTimelines
            .AnyAsync(x => x.PaymentId == evt.PaymentId, context.CancellationToken);

        if (!exists)
        {
            _db.PaymentTimelines.Add(new PaymentTimeline(evt.PaymentId));
            await _db.SaveChangesAsync(context.CancellationToken);
        }

        // 2️⃣ STATUS UPDATE (TRACKING YOK)
        await _db.PaymentTimelines
            .Where(x => x.PaymentId == evt.PaymentId)
            .ExecuteUpdateAsync(
                s => s.SetProperty(x => x.CurrentStatus, "Succeeded"),
                context.CancellationToken);

        // 3️⃣ EVENT INSERT
        _db.PaymentTimelineEvents.Add(
            new PaymentTimelineEvent(
                evt.PaymentId,
                "PaymentCompleted",
                string.IsNullOrWhiteSpace(evt.ProviderTransactionId)
                    ? "Payment completed."
                    : $"Payment completed. ProviderTransactionId: {evt.ProviderTransactionId}"));

        await _db.SaveChangesAsync(context.CancellationToken);

        await ReadModelIdempotency.MarkAsProcessedAsync(
            _db, messageId, consumerName, context.CancellationToken);
    }

}
