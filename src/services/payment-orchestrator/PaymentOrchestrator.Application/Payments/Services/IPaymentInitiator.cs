namespace PaymentOrchestrator.Application.Payments.Services;

public interface IPaymentInitiator
{
    Task InitiateAsync(ProviderPaymentRequest request);
}
