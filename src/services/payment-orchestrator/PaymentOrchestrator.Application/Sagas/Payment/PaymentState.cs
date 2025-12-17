using MassTransit;

namespace PaymentOrchestrator.Application.Sagas.Payment;

public sealed class PaymentState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = default!;

    // ---- Payment context (PaymentCreated'dan gelir) ----
    public int PaymentId { get; set; }
    public string MerchantId { get; set; } = default!;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = default!;

    // ---- TIMEOUT SCHEDULE IDS ----
    public Guid? FraudTimeoutTokenId { get; set; }
    public Guid? ProviderTimeoutTokenId { get; set; }

    public DateTime CreatedAt { get; set; }
}
