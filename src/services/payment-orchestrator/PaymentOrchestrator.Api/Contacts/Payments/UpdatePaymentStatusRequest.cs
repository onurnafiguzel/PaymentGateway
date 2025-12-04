using PaymentOrchestrator.Domain.Payments;

namespace PaymentOrchestrator.Api.Contracts.Payments;

public sealed class UpdatePaymentStatusRequest
{
    public PaymentStatus Status { get; set; }
}
