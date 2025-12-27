using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using PaymentOrchestrator.Application.Payments.Commands.UpdatePaymentStatus;
using PaymentOrchestrator.Domain.Payments;
using Shared.Messaging.Events.Fraud;

namespace PaymentOrchestrator.Application.Payments.Consumers;

public sealed class FraudCheckCompletedConsumer(
    IMediator mediator,
    ILogger<FraudCheckCompletedConsumer> logger)
    : ConsumerBase<FraudCheckCompletedEvent>(logger),
      IConsumer<FraudCheckCompletedEvent>
{

    public async Task Consume(ConsumeContext<FraudCheckCompletedEvent> context)
    {
        using (BeginConsumeScope(context, context.Message.PaymentId))
        {
            _logger.LogInformation("Consumer start");

            try
            {
                _logger.LogInformation(
                    "Fraud check completed | IsFraud={IsFraud} | Reason={Reason}",
                    context.Message.IsFraud,
                    context.Message.Reason);

                if (!context.Message.IsFraud)
                    return;

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
