using MassTransit;
using PaymentOrchestrator.Application.Abstractions;
using PaymentOrchestrator.Application.Persistence;
using Shared.Messaging.Events.Payments;

namespace PaymentOrchestrator.Application.Payments.Consumers;

public class PaymentCompletedConsumer(
    IPaymentRepository paymentRepository,
    IPublishEndpoint publishEndpoint,
    IUnitOfWork unitOfWork) : IConsumer<PaymentCompletedEvent>
{
    public async Task Consume(ConsumeContext<PaymentCompletedEvent> context)
    {
        var evt = context.Message;
        var consumerName = nameof(PaymentCompletedConsumer);           

        var payment = await paymentRepository.GetByIdAsync(evt.PaymentId);
        if (payment is null)
            return;

        payment.MarkAsCompleted(evt.ProviderTransactionId); 

        Console.WriteLine($"[ORCH] Payment {evt.PaymentId} marked completed.");
    }
}
