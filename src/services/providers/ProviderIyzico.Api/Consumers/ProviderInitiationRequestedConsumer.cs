using MassTransit;
using Shared.Messaging.Events.Payments;

namespace ProviderIyzico.Application.Consumers;

public class ProviderInitiationRequestedConsumer : IConsumer<ProviderInitiationRequestedEvent>
{
    public async Task Consume(ConsumeContext<ProviderInitiationRequestedEvent> context)
    {
        var evt = context.Message;

        Console.WriteLine($"[PAYTR] Initiation received for payment {evt.PaymentId}");

        // Fake provider process
        await Task.Delay(500); // simulate API call

        var providerTransactionId = Guid.NewGuid().ToString();

        await context.Publish(new PaymentCompletedEvent(
            evt.CorrelationId,
            evt.PaymentId,
            evt.MerchantId,
            evt.Amount,
            evt.Currency,
            providerTransactionId
        ));

        Console.WriteLine($"[PAYTR] PaymentCompletedEvent published for payment {evt.PaymentId}");
    }
}
