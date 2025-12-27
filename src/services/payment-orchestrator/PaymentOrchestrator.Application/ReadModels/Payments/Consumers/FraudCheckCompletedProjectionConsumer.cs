using MassTransit;
using Microsoft.EntityFrameworkCore;
using PaymentOrchestrator.Application.ReadModels.Common;
using PaymentOrchestrator.Application.ReadModels.Payments.Abstractions;
using Shared.Messaging.Events.Fraud;

namespace PaymentOrchestrator.Application.ReadModels.Payments.Consumers;

public sealed class FraudCheckCompletedProjectionConsumer
    : IConsumer<FraudCheckCompletedEvent>
{
    private readonly IPaymentReadDbContext _db;

    public FraudCheckCompletedProjectionConsumer(IPaymentReadDbContext db)
    {
        _db = db;
    }

    public async Task Consume(ConsumeContext<FraudCheckCompletedEvent> context)
    {
        var evt = context.Message;
        var messageId = context.MessageId ?? Guid.Empty;
        var consumerName = nameof(FraudCheckCompletedProjectionConsumer);


        if (await ReadModelIdempotency.IsAlreadyProcessedAsync(
          _db, messageId, consumerName, context.CancellationToken))
            return;

        var timeline = await _db.PaymentTimelines
            .Include(x => x.Events)
            .FirstOrDefaultAsync(x => x.PaymentId == evt.PaymentId);

        if (timeline is null) return;

        timeline.AddEvent(
            "FraudCheckCompleted",
            evt.IsFraud
                ? $"Fraud detected: {evt.Reason}"
                : "Fraud check passed");

        await _db.SaveChangesAsync();

        await ReadModelIdempotency.MarkAsProcessedAsync(
            _db, messageId, consumerName, context.CancellationToken);
    }
}
