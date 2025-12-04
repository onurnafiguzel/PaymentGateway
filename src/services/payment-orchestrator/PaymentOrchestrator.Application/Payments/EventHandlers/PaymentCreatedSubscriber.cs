using PaymentOrchestrator.Application.Common.Events;
using PaymentOrchestrator.Application.Payments.Services;
using Shared.Messaging.Events.Payments;

namespace PaymentOrchestrator.Application.Payments.EventHandlers;

public sealed class PaymentCreatedSubscriber
{
    private readonly IProviderSelector _providerSelector;
    private readonly IPaymentInitiator _paymentInitiator;

    public PaymentCreatedSubscriber(
        IProviderSelector providerSelector,
        IPaymentInitiator paymentInitiator,
        IEventBus eventBus)
    {
        _providerSelector = providerSelector;
        _paymentInitiator = paymentInitiator;

        eventBus.SubscribeAsync<PaymentCreatedEvent>(HandleAsync);
    }

    private async Task HandleAsync(PaymentCreatedEvent evt)
    {
        // 1. Provider seçimi
        var providerName = _providerSelector.Select(
            evt.MerchantId,
            evt.Amount,
            evt.Currency);

        // 2. Seçilen provider'a ödeme başlatma isteği gönder
        await _paymentInitiator.InitiateAsync(new ProviderPaymentRequest(
            evt.PaymentId,
            evt.MerchantId,
            evt.Amount,
            evt.Currency,
            providerName
        ));
    }
}
