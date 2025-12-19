using MassTransit;
using Microsoft.Extensions.Logging;
using PaymentOrchestrator.Application.Abstractions;
using PaymentOrchestrator.Application.Persistence;
using Shared.Messaging.Events.Fraud;
using Shared.Messaging.Events.Payments;

namespace PaymentOrchestrator.Application.Payments.Consumers;

public class FraudCheckCompletedConsumer(
    IPaymentRepository paymentRepository,
    IPublishEndpoint publishEndpoint,
    ILogger<FraudCheckCompletedConsumer> _logger,
    IUnitOfWork unitOfWork) : IConsumer<FraudCheckCompletedEvent>
{
    public async Task Consume(ConsumeContext<FraudCheckCompletedEvent> context)
    {
        _logger.LogInformation(
           "Consumed FraudCheckCompletedEvent | PaymentId={PaymentId} | CorrelationId={CorrelationId}",
           context.Message.PaymentId,
           context.Message.CorrelationId);

        var evt = context.Message;

        var payment = await paymentRepository.GetByIdAsync(evt.PaymentId);
        if (payment == null) return;

        if (evt.IsFraud)
        {
            payment.MarkAsFailed(evt.Reason ?? "fraud_detected");

            await publishEndpoint.Publish(new PaymentFailedEvent(
                evt.CorrelationId,
                payment.Id,
                evt.Reason ?? "fraud_detected"
                ));
        }

        await unitOfWork.SaveChangesAsync(context.CancellationToken);

        Console.WriteLine("[ORCH] Fraud OK → Provider process continues.");
    }
}