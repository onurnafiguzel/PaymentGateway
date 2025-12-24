using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using PaymentOrchestrator.Application.Payments.Commands.UpdatePaymentStatus;
using PaymentOrchestrator.Domain.Payments;
using Shared.Messaging.Events.Payments;

namespace PaymentOrchestrator.Application.Payments.Consumers;

public sealed class PaymentFailedConsumer(
    IMediator mediator,
    ILogger<PaymentFailedConsumer> logger
) : IConsumer<PaymentFailedEvent>
{
    public async Task Consume(ConsumeContext<PaymentFailedEvent> context)
    {
        var evt = context.Message;

        logger.LogInformation(
            "PaymentFailedEvent consumed | PaymentId={PaymentId} | Reason={Reason}",
            evt.PaymentId,
            evt.Reason);

        await mediator.Send(new UpdatePaymentStatusCommand(
            evt.PaymentId,
            PaymentStatus.Failed
        ));
    }
}

