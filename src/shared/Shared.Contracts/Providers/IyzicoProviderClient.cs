using Shared.Contracts.Providers;

namespace PaymentOrchestrator.Infrastructure.Providers;

public sealed class IyzicoProviderClient : IProviderClient
{
    public Task<ProviderResult> ProcessAsync(ProviderRequest request, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"[IYZICO] Payment processing... PaymentId={request.PaymentId}");

        return Task.FromResult(new ProviderResult
        {
            Success = true,
            ProviderTransactionId = "iyzico_" + Guid.NewGuid().ToString("N")
        });
    }
}
