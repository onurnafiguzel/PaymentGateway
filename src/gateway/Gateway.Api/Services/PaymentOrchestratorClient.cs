using Gateway.Api.Models.Payments;

namespace Gateway.Api.Services;

public interface IPaymentOrchestratorClient
{
    Task<PaymentResponse?> GetByIdAsync(int id);
    Task<IEnumerable<PaymentResponse>> GetAllAsync();
    Task<PaymentResponse?> CreateAsync(CreatePaymentRequest request);
    Task<bool> UpdateStatusAsync(int id, string status);
}

public class PaymentOrchestratorClient(HttpClient http) : IPaymentOrchestratorClient
{
    public async Task<PaymentResponse?> GetByIdAsync(int id)
        => await http.GetFromJsonAsync<PaymentResponse>($"api/payments/{id}");

    public async Task<IEnumerable<PaymentResponse>> GetAllAsync()
        => await http.GetFromJsonAsync<IEnumerable<PaymentResponse>>($"api/payments")
           ?? Enumerable.Empty<PaymentResponse>();

    public async Task<PaymentResponse?> CreateAsync(CreatePaymentRequest request)
    {
        var response = await http.PostAsJsonAsync("api/payments", request);
        return await response.Content.ReadFromJsonAsync<PaymentResponse>();
    }

    public async Task<bool> UpdateStatusAsync(int id, string status)
    {
        var response = await http.PutAsJsonAsync($"api/payments/{id}/status",
            new UpdatePaymentStatusRequest(status));

        return response.IsSuccessStatusCode;
    }
}
