using Shared.Contracts.Fraud;
using Shared.Messaging.Events.Common;
using Shared.Messaging.Events.Payments;

namespace FraudService.Api.Subscriber;

public class FraudPaymentSubscriber
{
    private readonly IEventBus _eventBus;

    public FraudPaymentSubscriber(IEventBus eventBus)
    {            
        _eventBus = eventBus;
        eventBus.SubscribeAsync<PaymentCreatedEvent>(HandleAsync);
    }

    private async Task HandleAsync(PaymentCreatedEvent evt)
    {
        var isFraud = Random.Shared.Next(0, 100) < 30;

        var fraudEvent = new FraudCheckCompletedEvent(
            evt.PaymentId,
            isFraud,
            isFraud ? "Suspicious activity detected" : null,
            DateTime.UtcNow
        );

        Console.WriteLine($"[FraudService] Payment {evt.PaymentId} Fraud = {isFraud}");

        await _eventBus.PublishAsync(fraudEvent);
    }
}

