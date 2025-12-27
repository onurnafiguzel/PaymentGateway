using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.Messaging.Events.Payments;

namespace PaymentOrchestrator.Application.Payments.Consumers;

public sealed class PaymentCreatedConsumer(
    ILogger<PaymentCreatedConsumer> logger)
    : ConsumerBase<PaymentCreatedEvent>(logger),
      IConsumer<PaymentCreatedEvent>
{

    public Task Consume(ConsumeContext<PaymentCreatedEvent> context)
    {
        using (BeginConsumeScope(context, context.Message.PaymentId))
        {
            _logger.LogInformation("Consumer start");

            _logger.LogInformation("Payment created event received");

            _logger.LogInformation("Consumer end");
            return Task.CompletedTask;
        }
    }
}
