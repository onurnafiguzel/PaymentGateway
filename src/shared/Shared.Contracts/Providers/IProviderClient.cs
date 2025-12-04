namespace Shared.Contracts.Providers;

public interface IProviderClient
{
    Task<ProviderResult> ProcessAsync(ProviderRequest request, CancellationToken cancellationToken = default);
}
