using MassTransit;
using Shared.Messaging.Events.Fraud;
using Shared.Messaging.Events.Payments;

namespace PaymentOrchestrator.Application.Sagas.Payment;

public sealed class PaymentStateMachine
    : MassTransitStateMachine<PaymentState>
{
    public State FraudChecking { get; private set; } = default!;
    public State ProviderInitiated { get; private set; } = default!;
    public State Completed { get; private set; } = default!;

    public Event<PaymentCreatedEvent> PaymentCreated { get; private set; } = default!;
    public Event<FraudCheckCompletedEvent> FraudCheckCompleted { get; private set; } = default!;
    public Event<PaymentCompletedEvent> PaymentCompleted { get; private set; } = default!;

    public PaymentStateMachine()
    {
        InstanceState(x => x.CurrentState);

        // ---- Correlation (header-based) ----
        Event(() => PaymentCreated, x =>
        {
            x.CorrelateById(ctx => ctx.CorrelationId!.Value);
            x.InsertOnInitial = true;
        });

        Event(() => FraudCheckCompleted, x =>
        {
            x.CorrelateById(ctx => ctx.CorrelationId!.Value);
        });

        Event(() => PaymentCompleted, x =>
        {
            x.CorrelateById(ctx => ctx.CorrelationId!.Value);
        });

        // ---- INITIAL ----
        Initially(
            When(PaymentCreated)
                .Then(ctx =>
                {
                    ctx.Instance.PaymentId = ctx.Message.PaymentId;
                    ctx.Instance.MerchantId = ctx.Message.MerchantId;
                    ctx.Instance.Amount = ctx.Message.Amount;
                    ctx.Instance.Currency = ctx.Message.Currency;
                    ctx.Instance.CreatedAt = ctx.Message.CreatedAt;
                })
                .TransitionTo(FraudChecking)
                .Publish(ctx => new StartFraudCheckEvent(
                    ctx.Instance.PaymentId,
                    ctx.Instance.MerchantId,
                    ctx.Instance.Amount,
                    ctx.Instance.Currency
                ))
        );

        // ---- FRAUD CHECK ----
        During(FraudChecking,
            When(FraudCheckCompleted)
                .IfElse(
                    ctx => ctx.Message.IsFraud,
                    fraud => fraud
                        .TransitionTo(Completed)
                        .Publish(ctx => new PaymentFailedEvent(
                            ctx.Instance.PaymentId,
                            ctx.Message.Reason ?? "FRAUD_DETECTED"
                        )),
                    clean => clean
                        .TransitionTo(ProviderInitiated)
                        .Publish(ctx => new ProviderInitiationRequestedEvent(
                            ctx.Instance.PaymentId,
                            ctx.Instance.MerchantId,
                            ctx.Instance.Amount,
                            ctx.Instance.Currency,
                            "Iyzico"
                        ))
                )
        );

        // ---- PROVIDER ----
        During(ProviderInitiated,
            When(PaymentCompleted)
                .TransitionTo(Completed)
        );
    }
}
