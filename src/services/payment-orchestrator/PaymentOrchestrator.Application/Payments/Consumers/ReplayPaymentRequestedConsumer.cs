using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using PaymentOrchestrator.Application.Payments.Commands.ReplayPayment;
using Shared.Messaging.Events.Payments;

namespace PaymentOrchestrator.Application.Payments.Consumers;

public sealed class ReplayPaymentRequestedConsumer(
    IMediator mediator,
    ILogger<ReplayPaymentRequestedConsumer> logger
) : IConsumer<ReplayPaymentRequestedEvent>
{
    public async Task Consume(ConsumeContext<ReplayPaymentRequestedEvent> context)
    {
        var evt = context.Message;

        logger.LogInformation(
            "ReplayPaymentRequestedEvent consumed | PaymentId={PaymentId} | Reason={Reason}",
            evt.PaymentId,
            evt.Reason);

        await mediator.Send(new ReplayPaymentCommand(
            evt.PaymentId,
            evt.Reason
        ), context.CancellationToken);
    }
}
