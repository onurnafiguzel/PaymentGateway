using Shared.Contracts.Providers;

namespace PaymentOrchestrator.Infrastructure.Providers;

public sealed class StripeProviderClient : IProviderClient
{
    public Task<ProviderResult> ProcessAsync(ProviderRequest request, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"[STRIPE] Payment processing... PaymentId={request.PaymentId}");

        return Task.FromResult(new ProviderResult
        {
            Success = true,
            ProviderTransactionId = "stripe_" + Guid.NewGuid().ToString("N")
        });
    }
}
