using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using PaymentOrchestrator.Application.Payments.Services;
using Shared.Contracts.Providers;
using Shared.Messaging.Events.Payments;

namespace PaymentOrchestrator.Application.Payments.Consumers;

public class ProviderInitiationConsumer(IServiceProvider provider) : IConsumer<ProviderInitiationRequestedEvent>
{
    public async Task Consume(ConsumeContext<ProviderInitiationRequestedEvent> context)
    {
        using var scope = provider.CreateScope();
        var initiator = scope.ServiceProvider.GetRequiredService<IPaymentInitiator>();

        await initiator.InitiateAsync(new ProviderPaymentRequest(
            context.Message.PaymentId,
            context.Message.MerchantId,
            context.Message.Amount,
            context.Message.Currency,
            context.Message.ProviderName
        ));

        Console.WriteLine("[ORCH] Provider initiation request sent.");
    }
}
