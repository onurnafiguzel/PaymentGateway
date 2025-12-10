using MassTransit;
using PaymentOrchestrator.Application.Abstractions;
using PaymentOrchestrator.Application.Persistence;
using Shared.Contracts.Payments;

namespace PaymentOrchestrator.Application.Payments.Consumers;

public class PaymentCompletedConsumer(IPaymentRepository repo, IUnitOfWork uow) : IConsumer<PaymentCompletedEvent>
{
    public async Task Consume(ConsumeContext<PaymentCompletedEvent> context)
    {
        var evt = context.Message;

        var payment = await repo.GetByIdAsync(evt.PaymentId);
        if (payment is null)
            return;

        payment.MarkAsCompleted(evt.ProviderTransactionId);
        await uow.SaveChangesAsync();

        Console.WriteLine($"[ORCH] Payment {evt.PaymentId} marked completed.");
    }
}
