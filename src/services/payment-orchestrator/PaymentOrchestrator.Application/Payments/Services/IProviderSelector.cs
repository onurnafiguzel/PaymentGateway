namespace PaymentOrchestrator.Application.Payments.Services;

public interface IProviderSelector
{
    string Select(string merchantId, decimal amount, string currency);
}
