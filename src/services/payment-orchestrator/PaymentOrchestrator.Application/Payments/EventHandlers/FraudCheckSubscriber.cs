using PaymentOrchestrator.Application.Abstractions;
using PaymentOrchestrator.Application.Common.Events;
using PaymentOrchestrator.Application.Persistence;
using Shared.Contracts.Fraud;

namespace PaymentOrchestrator.Application.Payments.EventHandlers;

public sealed class FraudCheckSubscriber
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IUnitOfWork _uow;

    public FraudCheckSubscriber(
        IPaymentRepository paymentRepository,
        IUnitOfWork uow,
        IEventBus eventBus)
    {
        _paymentRepository = paymentRepository;
        _uow = uow;

        eventBus.SubscribeAsync<FraudCheckCompletedEvent>(HandleAsync);
    }

    private async Task HandleAsync(FraudCheckCompletedEvent evt)
    {
        // 1. Payment'ı DB’den bul
        var payment = await _paymentRepository.GetByIdAsync(evt.PaymentId);
        if (payment is null)
            return;

        // 2. Fraud varsa Payment'ı işaretle
        if (evt.IsFraud)
        {
            payment.MarkAsFailed(evt.Reason ?? "fraud_detected");
            await _uow.SaveChangesAsync();
            return;
        }

        // Fraud değilse şimdilik hiçbir şey yapmıyoruz
        // Future: log, domain event, suspicious flag eklenebilir
    }
}
