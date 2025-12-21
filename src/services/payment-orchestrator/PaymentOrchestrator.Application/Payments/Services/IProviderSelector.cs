namespace PaymentOrchestrator.Application.Payments.Services;

public interface IProviderSelector
{
    string Select(string currency);
}
