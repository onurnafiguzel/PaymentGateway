namespace PaymentOrchestrator.Domain.Payments;

public enum PaymentStatus
{
    Pending = 0,
    FraudChecking = 1,
    ProviderInitiated = 2,
    Succeeded = 3,
    Failed = 4
}
