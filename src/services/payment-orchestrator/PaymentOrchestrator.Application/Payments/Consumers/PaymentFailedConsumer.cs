using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using PaymentOrchestrator.Application.Payments.Commands.UpdatePaymentStatus;
using PaymentOrchestrator.Domain.Payments;
using Shared.Messaging.Events.Payments;

namespace PaymentOrchestrator.Application.Payments.Consumers;

public sealed class PaymentFailedConsumer(
    IMediator mediator,
    ILogger<PaymentFailedConsumer> logger)
    : ConsumerBase<PaymentFailedEvent>(logger),
      IConsumer<PaymentFailedEvent>
{ 

    public async Task Consume(ConsumeContext<PaymentFailedEvent> context)
    {
        using (BeginConsumeScope(context, context.Message.PaymentId))
        {
            _logger.LogInformation("Consumer start");

            try
            {
                _logger.LogInformation(
                    "Payment failed | Reason={Reason}",
                    context.Message.Reason);

                await mediator.Send(new UpdatePaymentStatusCommand(
                    context.Message.PaymentId,
                    PaymentStatus.Failed
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Consumer failed");
                throw;
            }
            finally
            {
                _logger.LogInformation("Consumer end");
            }
        }
    }
}
