using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using PaymentOrchestrator.Application.Payments.Commands.UpdatePaymentStatus;
using PaymentOrchestrator.Domain.Payments;
using Shared.Messaging.Events.Payments;

namespace PaymentOrchestrator.Application.Payments.Consumers;

public sealed class PaymentCompletedConsumer(
    IMediator mediator,
    ILogger<PaymentCompletedConsumer> logger)
    : ConsumerBase<PaymentCompletedEvent>(logger),
      IConsumer<PaymentCompletedEvent>
{  

    public async Task Consume(ConsumeContext<PaymentCompletedEvent> context)
    {
        using (BeginConsumeScope(context, context.Message.PaymentId))
        {
            _logger.LogInformation("Consumer start");

            try
            {
                await mediator.Send(new UpdatePaymentStatusCommand(
                    context.Message.PaymentId,
                    PaymentStatus.Succeeded
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
