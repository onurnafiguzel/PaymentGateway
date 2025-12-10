using MassTransit;
using Shared.Messaging.Events.Payments;
using Shared.Messaging.Events.Fraud;
using Shared.Contracts.Events.Fraud;
using Shared.Contracts.Payments;

namespace PaymentOrchestrator.Application.Payments.Consumers;

public class PaymentCreatedConsumer : IConsumer<PaymentCreatedEvent>
{
    public async Task Consume(ConsumeContext<PaymentCreatedEvent> context)
    {
        var evt = context.Message;

        Console.WriteLine($"[ORCH] PaymentCreated received → FraudCheckRequested published");

        await context.Publish(new FraudCheckRequestedEvent(
            evt.PaymentId,
            evt.MerchantId,
            evt.Amount
        ));

        Console.WriteLine($"[ORCH] PaymentCreated received → Provider initiation trigger published");

        await context.Publish(new ProviderInitiationRequestedEvent(
            evt.PaymentId,
            evt.MerchantId,
            evt.Amount,
            evt.Currency,
            "Iyzico" // TODO
        ));
    }
}
