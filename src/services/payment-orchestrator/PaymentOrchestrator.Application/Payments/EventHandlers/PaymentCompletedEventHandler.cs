using PaymentOrchestrator.Application.Abstractions;
using PaymentOrchestrator.Application.Persistence;
using Shared.Contracts.Payments;
using Shared.Messaging.Events.Common;

namespace PaymentOrchestrator.Application.Payments.EventHandlers;

public sealed class PaymentCompletedEventHandler
{
    private IPaymentRepository _paymentRepository;
    private IUnitOfWork _uow;

    public PaymentCompletedEventHandler(
        IPaymentRepository paymentRepository,
        IUnitOfWork uow,
        IEventBus eventBus)
    {
        _paymentRepository = paymentRepository;
        _uow = uow;

        eventBus.SubscribeAsync<PaymentCompletedEvent>(HandleAsync);
    }

    private async Task HandleAsync(PaymentCompletedEvent evt)
    {
        var payment = await _paymentRepository.GetByIdAsync(evt.PaymentId);
        if (payment is null)
            return;

        payment.MarkAsCompleted(evt.ProviderTransactionId);

        await _uow.SaveChangesAsync();
    }
}


