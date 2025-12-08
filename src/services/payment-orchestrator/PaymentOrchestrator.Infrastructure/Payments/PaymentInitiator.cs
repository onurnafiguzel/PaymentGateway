using PaymentOrchestrator.Application.Payments.Services;
using Shared.Contracts.Providers;
using System.Net.Http.Json;

public class PaymentInitiator(HttpClient http) : IPaymentInitiator
{
    public async Task InitiateAsync(ProviderPaymentRequest request)
    {
        var url = request.ProviderName switch
        {
            "PAYTR" => "http://localhost:5011/api/provider-paytr/initiate",
            "STRIPE" => "http://localhost:5012/api/provider-stripe/initiate",
            "IYZICO" => "http://localhost:5013/api/provider-iyzico/initiate",
            _ => throw new Exception("Unknown provider")
        };

        await http.PostAsJsonAsync(url, request);
    }
}
