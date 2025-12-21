using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using PaymentOrchestrator.Application.Payments.Services;
using Shared.Contracts.Providers;
using Shared.Messaging.Events.Payments;

namespace PaymentOrchestrator.Application.Payments.Consumers;

public class ProviderInitiationConsumer(IServiceProvider provider, IProviderSelector providerSelector) : IConsumer<ProviderInitiationRequestedEvent>
{
    public async Task Consume(ConsumeContext<ProviderInitiationRequestedEvent> context)
    {
        using var scope = provider.CreateScope();
        var initiator = scope.ServiceProvider.GetRequiredService<IPaymentInitiator>();
        var providerName = providerSelector.Select(context.Message.Currency);
        
        await initiator.InitiateAsync(new ProviderPaymentRequest(
            context.Message.PaymentId,
            context.Message.MerchantId,
            context.Message.Amount,
            context.Message.Currency,
            providerName
        ));

        Console.WriteLine("[ORCH] Provider initiation request sent.");
    }
}
