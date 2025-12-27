using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using PaymentOrchestrator.Application.Payments.Commands.ReplayPayment;
using Shared.Messaging.Events.Payments;

namespace PaymentOrchestrator.Application.Payments.Consumers;

public sealed class ReplayPaymentRequestedConsumer(
     IMediator mediator,
     ILogger<ReplayPaymentRequestedConsumer> logger)
    : ConsumerBase<ReplayPaymentRequestedEvent>(logger),
      IConsumer<ReplayPaymentRequestedEvent>
{

    public async Task Consume(ConsumeContext<ReplayPaymentRequestedEvent> context)
    {
        using (BeginConsumeScope(context, context.Message.PaymentId))
        {
            _logger.LogInformation("Consumer start");

            try
            {
                _logger.LogInformation(
                    "Replay payment requested | Reason={Reason}",
                    context.Message.Reason);

                await mediator.Send(
                    new ReplayPaymentCommand(
                        context.Message.PaymentId,
                        context.Message.Reason),
                    context.CancellationToken);
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
