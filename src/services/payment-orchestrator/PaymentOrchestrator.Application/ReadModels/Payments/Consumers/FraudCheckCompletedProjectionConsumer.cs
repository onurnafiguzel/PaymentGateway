using MassTransit;
using Microsoft.Extensions.Logging;
using PaymentOrchestrator.Application.ReadModels.Common;
using PaymentOrchestrator.Application.ReadModels.Payments.Abstractions;
using Shared.Messaging.Events.Fraud;

namespace PaymentOrchestrator.Application.ReadModels.Payments.Consumers;

public sealed class FraudCheckCompletedProjectionConsumer
    : IConsumer<FraudCheckCompletedEvent>
{
    private readonly IPaymentTimelineProjectionWriter _writer;
    private readonly IPaymentReadDbContext _db;
    private readonly ILogger<FraudCheckCompletedProjectionConsumer> _logger;


    public FraudCheckCompletedProjectionConsumer(
        IPaymentTimelineProjectionWriter writer,
        IPaymentReadDbContext db,
        ILogger<FraudCheckCompletedProjectionConsumer> logger
        )
    {
        _writer = writer;
        _db = db;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<FraudCheckCompletedEvent> context)
    {
        _logger.LogInformation(
            "FraudCheckCompletedProjectionConsumer start | PaymentId={PaymentId}",
            context.Message.PaymentId);

        var evt = context.Message;
        var consumerName = nameof(FraudCheckCompletedProjectionConsumer);
        var messageId = context.MessageId ?? Guid.Empty;

        if (await ReadModelIdempotency.IsAlreadyProcessedAsync(
                _db, messageId, consumerName, context.CancellationToken))
            return;

        await _writer.EnsureTimelineExistsAsync(evt.PaymentId, context.CancellationToken);

        await _writer.AddEventAsync(
            evt.PaymentId,
            "FraudCheckCompleted",
            evt.IsFraud
                ? $"Fraud detected: {evt.Reason}"
                : "Fraud check passed",
            context.CancellationToken);

        await ReadModelIdempotency.MarkAsProcessedAsync(
            _db, messageId, consumerName, context.CancellationToken);

        _logger.LogInformation(
            "FraudCheckCompletedProjectionConsumer end | PaymentId={PaymentId}",
            context.Message.PaymentId);
    }
}

