using MassTransit;
using Shared.Contracts.Payments;

namespace ProviderStripe.Api.Consumers;

public class ProviderInitiationRequestedConsumer : IConsumer<ProviderInitiationRequestedEvent>
{
    public async Task Consume(ConsumeContext<ProviderInitiationRequestedEvent> context)
    {
        var evt = context.Message;

        Console.WriteLine($"[STRIPE] Initiation received for payment {evt.PaymentId}");

        // Fake provider process
        await Task.Delay(500); // simulate API call

        var providerTransactionId = Guid.NewGuid().ToString();

        await context.Publish(new PaymentCompletedEvent(
            evt.PaymentId,
            evt.MerchantId,
            evt.Amount,
            evt.Currency,
            providerTransactionId
        ));

        Console.WriteLine($"[STRIPE] PaymentCompletedEvent published for payment {evt.PaymentId}");
    }
}
