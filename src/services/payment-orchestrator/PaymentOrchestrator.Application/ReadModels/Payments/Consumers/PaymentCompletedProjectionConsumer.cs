using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaymentOrchestrator.Application.ReadModels.Common;
using PaymentOrchestrator.Application.ReadModels.Payments.Abstractions;
using Serilog.Core;
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
        _logger.LogInformation("PaymentCompletedProjectionConsumer consumed {PaymentId}", context.Message.PaymentId);

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
