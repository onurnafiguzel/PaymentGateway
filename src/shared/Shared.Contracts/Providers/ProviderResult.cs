namespace Shared.Contracts.Providers;

public sealed class ProviderResult
{
    public bool Success { get; init; }
    public string ProviderTransactionId { get; init; } = default!;
}
