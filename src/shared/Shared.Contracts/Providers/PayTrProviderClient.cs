using Shared.Contracts.Providers;

namespace PaymentOrchestrator.Infrastructure.Providers;

public sealed class PayTrProviderClient : IProviderClient
{
    public Task<ProviderResult> ProcessAsync(ProviderRequest request, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"[PAYTR] Payment processing... PaymentId={request.PaymentId}");

        return Task.FromResult(new ProviderResult
        {
            Success = true,
            ProviderTransactionId = "paytr_" + Guid.NewGuid().ToString("N")
        });
    }
}
