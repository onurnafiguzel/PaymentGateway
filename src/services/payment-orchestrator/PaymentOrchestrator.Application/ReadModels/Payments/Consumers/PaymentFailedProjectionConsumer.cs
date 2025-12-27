using MassTransit;
using Microsoft.Extensions.Logging;
using PaymentOrchestrator.Application.ReadModels.Common;
using PaymentOrchestrator.Application.ReadModels.Payments.Abstractions;
using Shared.Messaging.Events.Payments;

namespace PaymentOrchestrator.Application.ReadModels.Payments.Consumers;

public sealed class PaymentFailedProjectionConsumer
    : IConsumer<PaymentFailedEvent>
{
    private readonly IPaymentTimelineProjectionWriter _writer;
    private readonly IPaymentReadDbContext _db;
    private readonly ILogger<PaymentFailedProjectionConsumer> _logger;


    public PaymentFailedProjectionConsumer(
        IPaymentTimelineProjectionWriter writer,
        IPaymentReadDbContext db,
        ILogger<PaymentFailedProjectionConsumer> logger)
    {
        _writer = writer;
        _db = db;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentFailedEvent> context)
    {
        _logger.LogInformation(
            "PaymentFailedProjectionConsumer start | PaymentId={PaymentId}",
            context.Message.PaymentId);

        var evt = context.Message;
        var consumerName = nameof(PaymentFailedProjectionConsumer);
        var messageId = context.MessageId ?? Guid.Empty;

        if (await ReadModelIdempotency.IsAlreadyProcessedAsync(
                _db, messageId, consumerName, context.CancellationToken))
            return;

        await _writer.EnsureTimelineExistsAsync(evt.PaymentId, context.CancellationToken);
        await _writer.UpdateStatusAsync(evt.PaymentId, "Failed", context.CancellationToken);

        await _writer.AddEventAsync(
            evt.PaymentId,
            "PaymentFailed",
            $"Payment failed. Reason: {evt.Reason}",
            context.CancellationToken);

        await ReadModelIdempotency.MarkAsProcessedAsync(
            _db, messageId, consumerName, context.CancellationToken);

        _logger.LogInformation(
            "PaymentFailedProjectionConsumer end | PaymentId={PaymentId}",
            context.Message.PaymentId);
    }
}

