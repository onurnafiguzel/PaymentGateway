using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.Messaging.Events.Payments;

namespace PaymentOrchestrator.Application.Payments.Consumers;

public sealed class PaymentCreatedConsumer : IConsumer<PaymentCreatedEvent>
{
    private readonly ILogger<PaymentCreatedConsumer> _logger;

    public PaymentCreatedConsumer(ILogger<PaymentCreatedConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<PaymentCreatedEvent> context)
    {
        _logger.LogInformation(
            "PaymentCreatedEvent consumed | PaymentId={PaymentId} | CorrelationId={CorrelationId}",
            context.Message.PaymentId,
            context.Message.CorrelationId);

        // DB write YOK
        // Aggregate YOK
        // Saga zaten bu event'i dinliyor
        // Fraud flow'u tetikleniyor
        return Task.CompletedTask;
    }
}

