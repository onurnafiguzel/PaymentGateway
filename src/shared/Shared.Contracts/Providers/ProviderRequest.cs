namespace Shared.Contracts.Providers;

public sealed class ProviderRequest
{
    public int PaymentId { get; init; }
    public string MerchantId { get; init; } = default!;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = default!;
}
