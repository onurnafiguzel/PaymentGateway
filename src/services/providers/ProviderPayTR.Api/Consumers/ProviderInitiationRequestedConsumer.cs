using MassTransit;
using Shared.Contracts.Payments;

namespace ProviderPayTR.Consumers;

public class ProviderInitiationRequestedConsumer : IConsumer<ProviderInitiationRequestedEvent>
{
    public async Task Consume(ConsumeContext<ProviderInitiationRequestedEvent> context)
    {
        var evt = context.Message;

        Console.WriteLine($"[PAYTR] Provider initiation for payment {evt.PaymentId}");

        // Fake provider process
        await Task.Delay(300);

        var transactionId = Guid.NewGuid().ToString();

        await context.Publish(new PaymentCompletedEvent(
            evt.PaymentId,
            evt.MerchantId,
            evt.Amount,
            evt.Currency,
            transactionId
        ));

        Console.WriteLine($"[PAYTR] PaymentCompletedEvent published → {transactionId}");
    }
}
