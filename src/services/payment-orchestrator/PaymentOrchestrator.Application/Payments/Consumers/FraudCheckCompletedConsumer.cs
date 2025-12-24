using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using PaymentOrchestrator.Application.Payments.Commands.UpdatePaymentStatus;
using PaymentOrchestrator.Domain.Payments;
using Shared.Messaging.Events.Fraud;

namespace PaymentOrchestrator.Application.Payments.Consumers;

public sealed class FraudCheckCompletedConsumer(
    IMediator mediator,
    ILogger<FraudCheckCompletedConsumer> logger
) : IConsumer<FraudCheckCompletedEvent>
{
    public async Task Consume(ConsumeContext<FraudCheckCompletedEvent> context)
    {
        var evt = context.Message;

        logger.LogInformation(
            "FraudCheckCompletedEvent consumed | PaymentId={PaymentId} | IsFraud={IsFraud}",
            evt.PaymentId,
            evt.IsFraud);

        if (!evt.IsFraud)
            return; // Saga devam edecek, domain update yok

        await mediator.Send(new UpdatePaymentStatusCommand(
            evt.PaymentId,
            PaymentStatus.Failed
        ));
    }
}
