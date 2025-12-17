using MassTransit;
using Shared.Messaging.Events.Fraud;

namespace FraudService.Application.Consumers;

public sealed class StartFraudCheckConsumer
    : IConsumer<StartFraudCheckEvent>
{
    public async Task Consume(ConsumeContext<StartFraudCheckEvent> context)
    {
        var evt = context.Message;

        Console.WriteLine($"[FRAUD] Fraud check requested for payment {evt.PaymentId}");

        // Fake fraud logic
        bool isFraud = evt.Amount > 10_000;
        string? reason = isFraud ? "High amount fraud suspicion" : null;

        await context.Publish(new FraudCheckCompletedEvent(
            evt.PaymentId,
            isFraud,
            reason,
            DateTime.UtcNow
        ));

        Console.WriteLine($"[FRAUD] FraudCheckCompletedEvent published → Payment {evt.PaymentId}");
    }
}
