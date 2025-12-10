using MassTransit;
using Shared.Contracts.Events.Fraud;

namespace FraudService.Application.Consumers;

public class FraudCheckRequestedConsumer : IConsumer<FraudCheckRequestedEvent>
{
    public async Task Consume(ConsumeContext<FraudCheckRequestedEvent> context)
    {
        var evt = context.Message;

        Console.WriteLine($"[FRAUD] Fraud check requested for payment {evt.PaymentId}");

        // Fake fraud logic
        bool isFraud = evt.Amount > 10000;
        string? reason = isFraud ? "High amount fraud suspicion" : null;

        await context.Publish(new FraudCheckCompletedEvent(
            evt.PaymentId,
            evt.MerchantId,
            evt.Amount,
            isFraud,
            reason
        ));

        Console.WriteLine($"[FRAUD] FraudCheckCompletedEvent published → Payment {evt.PaymentId}");
    }
}
