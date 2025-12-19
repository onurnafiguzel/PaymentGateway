using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.Messaging.Events.Payments;

namespace PaymentOrchestrator.Application.Payments.Consumers;

/// <summary>
/// Observability & safety consumer.
/// Amaç:
/// - PaymentCreatedEvent gerçekten publish ediliyor mu?
/// - CorrelationId / PaymentId doğru mu?
/// - Saga'ya ulaşmadan önce event kayboluyor mu?
/// </summary>
public sealed class PaymentCreatedConsumer
    : IConsumer<PaymentCreatedEvent>
{
    private readonly ILogger<PaymentCreatedConsumer> _logger;

    public PaymentCreatedConsumer(
        ILogger<PaymentCreatedConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<PaymentCreatedEvent> context)
    {
        Console.WriteLine("🔥 PAYMENT CREATED CONSUMER HIT 🔥");

        var message = context.Message;

        _logger.LogInformation(
            "PaymentCreatedEvent consumed | PaymentId={PaymentId} | CorrelationId={CorrelationId} | MerchantId={MerchantId} | Amount={Amount} {Currency}",
            message.PaymentId,
            message.CorrelationId,
            message.MerchantId,
            message.Amount,
            message.Currency
        );

        // ❗ Burada business logic YOK
        // Saga bu event'i ayrıca consume edecek

        return Task.CompletedTask;
    }
}
