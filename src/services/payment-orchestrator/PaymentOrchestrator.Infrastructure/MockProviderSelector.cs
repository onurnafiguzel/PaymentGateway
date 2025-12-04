using PaymentOrchestrator.Application.Payments.Services;

public class MockProviderSelector : IProviderSelector
{
    public string Select(string merchantId, decimal amount, string currency)
    {
        if (currency == "TRY")
            return "PAYTR";

        if (currency == "USD" || currency == "EUR")
            return "STRIPE";

        return "IYZICO"; // fallback
    }
}
