using MassTransit;
using Shared.Messaging.Events.Fraud;

namespace FraudService.Api.Consumers;

public sealed class StartFraudCheckConsumer
    : IConsumer<StartFraudCheckEvent>
{
    private readonly ILogger<StartFraudCheckConsumer> _logger;
    private readonly IBus _bus;

    public StartFraudCheckConsumer(
        ILogger<StartFraudCheckConsumer> logger,
        IBus bus)
    {
        _logger = logger;
        _bus = bus;
    }

    public async Task Consume(ConsumeContext<StartFraudCheckEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Fraud check started | PaymentId={PaymentId} | Amount={Amount} | MerchantId={MerchantId} | CorrelationId={CorrelationId}",
            message.PaymentId,
            message.Amount,
            message.MerchantId,
            message.CorrelationId);

        // --------------------
        // FRAUD DECISION
        // --------------------
        bool isFraud =
            message.Amount > 10000 ||
            Convert.ToInt16(message.MerchantId) % 7 == 0;

        var reason = isFraud
            ? "RULE_BASED_FRAUD"
            : null;

        _logger.LogInformation(
            "Fraud check completed | PaymentId={PaymentId} | IsFraud={IsFraud} | Reason={Reason}",
            message.PaymentId,
            isFraud,
            reason);


        try
        {
            await _bus.Publish(new FraudCheckCompletedEvent(
                message.CorrelationId,
                message.PaymentId,
                isFraud,
                reason,
                DateTime.UtcNow
            ));
        }
        catch (Exception ex)
        {
            _logger.LogInformation("StartFraudCheckConsumer patladı");
            throw;
        }
    }
}
