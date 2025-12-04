using Microsoft.Extensions.DependencyInjection;
using PaymentOrchestrator.Application.Abstractions;
using PaymentOrchestrator.Application.Common.Events;
using PaymentOrchestrator.Application.Persistence;
using PaymentOrchestrator.Infrastructure.Providers;
using Shared.Contracts.Providers;
using Shared.Messaging.Events.Fraud;

namespace PaymentOrchestrator.Infrastructure.Subscribers;

public sealed class PaymentProviderSubscriber
{
    private readonly IServiceProvider _serviceProvider;

    public PaymentProviderSubscriber(IEventBus eventBus, IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        eventBus.SubscribeAsync<FraudResultEvent>(HandleAsync);
    }

    private async Task HandleAsync(FraudResultEvent @event)
    {
        using var scope = _serviceProvider.CreateScope();

        var paymentRepository = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var payment = await paymentRepository.GetByIdAsync(@event.PaymentId);
        if (payment is null)
            return;

        if (@event.IsFraud)
        {
            payment.MarkAsFailed("FRAUD");
            await unitOfWork.SaveChangesAsync();
            Console.WriteLine("[ORCHESTRATOR] Payment FAILED due to fraud.");
            return;
        }

        // Provider seçelim
        var provider = GetProvider(payment.MerchantId, scope);

        var providerResult = await provider.ProcessAsync(new ProviderRequest
        {
            PaymentId = payment.Id,
            MerchantId = payment.MerchantId,
            Amount = payment.Amount,
            Currency = payment.Currency
        });

        if (providerResult.Success)
            payment.MarkAsCompleted(providerResult.ProviderTransactionId);
        else
            payment.MarkAsFailed("PROVIDER_FAIL");

        await unitOfWork.SaveChangesAsync();
        Console.WriteLine("[ORCHESTRATOR] Payment completed.");
    }

    private IProviderClient GetProvider(string merchantId, IServiceScope scope)
    {
        if (merchantId.StartsWith("STR"))
            return scope.ServiceProvider.GetRequiredService<StripeProviderClient>();

        if (merchantId.StartsWith("PAY"))
            return scope.ServiceProvider.GetRequiredService<PayTrProviderClient>();

        return scope.ServiceProvider.GetRequiredService<IyzicoProviderClient>();
    }
}
