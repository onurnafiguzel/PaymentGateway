using Microsoft.Extensions.DependencyInjection;
using PaymentOrchestrator.Application.Abstractions;
using PaymentOrchestrator.Application.Persistence;
using Shared.Contracts.Fraud;
using Shared.Messaging.Events.Common;

namespace PaymentOrchestrator.Application.Payments.EventHandlers;

public sealed class FraudCheckSubscriber
{
    private readonly IServiceProvider _serviceProvider;

    public FraudCheckSubscriber(IEventBus eventBus, IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;

        // Fraud eventlerini dinliyoruz
        eventBus.SubscribeAsync<FraudCheckCompletedEvent>(HandleAsync);
    }

    private async Task HandleAsync(FraudCheckCompletedEvent evt)
    {
        // Scoped yaşam alanı açılır (Request-scope gibi)
        using var scope = _serviceProvider.CreateScope();

        // Scoped repository ve UnitOfWork çözülür
        var repo = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        // 1. Payment'ı bul
        var payment = await repo.GetByIdAsync(evt.PaymentId);
        if (payment is null)
            return;

        // 2. Fraud varsa işle
        if (evt.IsFraud)
        {
            payment.MarkAsFailed(evt.Reason ?? "fraud_detected");
            await uow.SaveChangesAsync();
            return;
        }

        // Fraud değilse: şimdilik hiçbir işlem yok (future: queue, log, saga step)
    }
}
