using MassTransit;
using PaymentOrchestrator.Application.Abstractions;
using PaymentOrchestrator.Application.Persistence;
using Shared.Contracts.Fraud;
using Shared.Contracts.Payments;

namespace PaymentOrchestrator.Application.Payments.Consumers;

public class FraudCheckCompletedConsumer(IPaymentRepository repo, IUnitOfWork uow) : IConsumer<FraudCheckCompletedEvent>
{
    public async Task Consume(ConsumeContext<FraudCheckCompletedEvent> context)
    {
        var evt = context.Message;

        var payment = await repo.GetByIdAsync(evt.PaymentId);
        if (payment == null) return;

        if (evt.IsFraud)
        {
            payment.MarkAsFailed(evt.Reason ?? "fraud");
            await uow.SaveChangesAsync();

            await context.Publish(new PaymentFailedEvent(evt.PaymentId, evt.Reason ?? "fraud"));
            return;
        }

        Console.WriteLine("[ORCH] Fraud OK → Provider process continues.");
    }
}