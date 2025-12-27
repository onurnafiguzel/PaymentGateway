using MassTransit;
using Microsoft.Extensions.Logging;
using PaymentOrchestrator.Application.ReadModels.Common;
using PaymentOrchestrator.Application.ReadModels.Payments.Abstractions;
using Shared.Messaging.Events.Payments;

namespace PaymentOrchestrator.Application.ReadModels.Payments.Consumers;

public sealed class PaymentCompletedProjectionConsumer
    : IConsumer<PaymentCompletedEvent>
{
    private readonly IPaymentTimelineProjectionWriter _writer;
    private readonly IPaymentReadDbContext _db;
    private readonly ILogger<PaymentCompletedProjectionConsumer> _logger;

    public PaymentCompletedProjectionConsumer(
        IPaymentTimelineProjectionWriter writer,
        IPaymentReadDbContext db,
        ILogger<PaymentCompletedProjectionConsumer> logger)
    {
        _writer = writer;
        _db = db;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentCompletedEvent> context)
    {
        _logger.LogInformation(
              "PaymentCompletedProjectionConsumer start | PaymentId={PaymentId}",
              context.Message.PaymentId);

        var evt = context.Message;
        var consumerName = nameof(PaymentCompletedProjectionConsumer);
        var messageId = context.MessageId ?? Guid.Empty;

        if (await ReadModelIdempotency.IsAlreadyProcessedAsync(
                _db, messageId, consumerName, context.CancellationToken))
            return;

        await _writer.EnsureTimelineExistsAsync(evt.PaymentId, context.CancellationToken);
        await _writer.UpdateStatusAsync(evt.PaymentId, "Succeeded", context.CancellationToken);

        await _writer.AddEventAsync(
            evt.PaymentId,
            "PaymentCompleted",
            string.IsNullOrWhiteSpace(evt.ProviderTransactionId)
                ? "Payment completed."
                : $"Payment completed. ProviderTransactionId: {evt.ProviderTransactionId}",
            context.CancellationToken);

        await ReadModelIdempotency.MarkAsProcessedAsync(
            _db, messageId, consumerName, context.CancellationToken);

        _logger.LogInformation(
            "PaymentCompletedProjectionConsumer end | PaymentId={PaymentId}",
            context.Message.PaymentId);
    }
}
