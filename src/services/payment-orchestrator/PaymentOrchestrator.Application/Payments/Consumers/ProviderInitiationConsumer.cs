using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PaymentOrchestrator.Application.Payments.Services;
using Shared.Contracts.Providers;
using Shared.Messaging.Events.Payments;

namespace PaymentOrchestrator.Application.Payments.Consumers;

public sealed class ProviderInitiationConsumer
    : ConsumerBase<ProviderInitiationRequestedEvent>,
      IConsumer<ProviderInitiationRequestedEvent>
{
    private readonly IServiceProvider _provider;
    private readonly IProviderSelector _providerSelector;

    public ProviderInitiationConsumer(
        IServiceProvider provider,
        IProviderSelector providerSelector,
        ILogger<ProviderInitiationConsumer> logger)
        : base(logger)
    {
        _provider = provider;
        _providerSelector = providerSelector;
    }

    public async Task Consume(ConsumeContext<ProviderInitiationRequestedEvent> context)
    {
        using (BeginConsumeScope(context, context.Message.PaymentId))
        {
            _logger.LogInformation("Consumer start");

            try
            {
                using var scope = _provider.CreateScope();
                var initiator = scope.ServiceProvider.GetRequiredService<IPaymentInitiator>();
                var providerName = _providerSelector.Select(context.Message.Currency);

                await initiator.InitiateAsync(new ProviderPaymentRequest(
                    context.Message.PaymentId,
                    context.Message.MerchantId,
                    context.Message.Amount,
                    context.Message.Currency,
                    providerName
                ));

                _logger.LogInformation(
                    "Provider initiation requested | Provider={Provider}",
                    providerName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Consumer failed");
                throw;
            }
            finally
            {
                _logger.LogInformation("Consumer end");
            }
        }
    }
}
