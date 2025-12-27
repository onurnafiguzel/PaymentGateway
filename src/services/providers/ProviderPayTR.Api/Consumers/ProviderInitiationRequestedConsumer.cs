using MassTransit;
using Shared.Messaging.Events.Payments;

namespace ProviderPayTR.Api.Consumers;

public sealed class ProviderInitiationRequestedConsumer
    : IConsumer<ProviderInitiationRequestedEvent>
{
    private readonly ILogger<ProviderInitiationRequestedConsumer> _logger;

    public ProviderInitiationRequestedConsumer(
        ILogger<ProviderInitiationRequestedConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ProviderInitiationRequestedEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "[PAYTR] Provider initiation received | PaymentId={PaymentId} | Amount={Amount} {Currency} | CorrelationId={CorrelationId}",
            message.PaymentId,
            message.Amount,
            message.Currency,
            message.CorrelationId);

        // 1️⃣ Gerçekçi provider davranışı (delay)
        var delaySeconds = Random.Shared.Next(2, 9);
        await Task.Delay(TimeSpan.FromSeconds(delaySeconds), context.CancellationToken);

        // 2️⃣ %15 ihtimalle provider fail simülasyonu
        var isFailed = Random.Shared.NextDouble() < 0.15;

        if (!isFailed)
        {
            _logger.LogInformation(
                "[PAYTR] Payment succeeded | PaymentId={PaymentId} | CorrelationId={CorrelationId}",
                message.PaymentId,
                message.CorrelationId);

            await context.Publish(new PaymentCompletedEvent(
                CorrelationId: message.CorrelationId,
                PaymentId: message.PaymentId,
                MerchantId: message.MerchantId,
                Amount: message.Amount,
                Currency: message.Currency,
                ProviderTransactionId: Guid.NewGuid().ToString()
            ));

            return;
        }

        _logger.LogWarning(
            "[PAYTR] Payment failed | PaymentId={PaymentId} | CorrelationId={CorrelationId}",
            message.PaymentId,
            message.CorrelationId);

        await context.Publish(new PaymentFailedEvent(
            CorrelationId: message.CorrelationId,
            PaymentId: message.PaymentId,
            Reason: "PROVIDER_PAYTR_DECLINED"
        ));
    }
}
