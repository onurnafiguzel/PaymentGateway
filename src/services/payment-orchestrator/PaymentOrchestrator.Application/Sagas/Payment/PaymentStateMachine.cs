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

    // ---- SCHEDULES ----
    public Schedule<PaymentState, FraudTimeoutExpiredEvent> FraudTimeout { get; private set; } = default!;
    public Schedule<PaymentState, ProviderTimeoutExpiredEvent> ProviderTimeout { get; private set; } = default!;


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

        // -------- SCHEDULE CONFIG --------
        Schedule(() => FraudTimeout, state => state.FraudTimeoutTokenId, s =>
        {
            s.Delay = TimeSpan.FromMinutes(2);
            s.Received = x => x.CorrelateById(ctx => ctx.CorrelationId!.Value);
        });

        Schedule(() => ProviderTimeout, state => state.ProviderTimeoutTokenId, s =>
        {
            s.Delay = TimeSpan.FromMinutes(5);
            s.Received = x => x.CorrelateById(ctx => ctx.CorrelationId!.Value);
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
                .Schedule(
                    FraudTimeout,
                    ctx => new FraudTimeoutExpiredEvent()
                )
                .TransitionTo(FraudChecking)
                .Publish(ctx => new StartFraudCheckEvent(
                    ctx.Instance.PaymentId,
                    ctx.Instance.MerchantId,
                    ctx.Instance.Amount,
                    ctx.Instance.Currency
                ))
        );

        // ---- FRAUD CHECKING ----
        During(FraudChecking,
            When(FraudCheckCompleted)
                .Unschedule(FraudTimeout)
                    .IfElse(
                        ctx => ctx.Message.IsFraud,
                        fraud => fraud
                            .TransitionTo(Completed)
                            .Publish(ctx => new PaymentFailedEvent(
                                ctx.Instance.PaymentId,
                                ctx.Message.Reason ?? "FRAUD_DETECTED"
                            )),
                        clean => clean
                            .Schedule(
                                ProviderTimeout,
                                ctx => new ProviderTimeoutExpiredEvent()
                            )
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
                .Unschedule(ProviderTimeout)
                .TransitionTo(Completed)
        );

        // -------- FRAUD TIMEOUT --------
        During(FraudChecking,
            When(FraudTimeout.Received)
                .TransitionTo(Completed)
                .Publish(ctx => new PaymentFailedEvent(
                    ctx.Instance.PaymentId,
                    "FRAUD_TIMEOUT"
                ))
        );

        // -------- PROVIDER TIMEOUT --------
        During(ProviderInitiated,
            When(ProviderTimeout.Received)
                .TransitionTo(Completed)
                .Publish(ctx => new PaymentFailedEvent(
                    ctx.Instance.PaymentId,
                    "PROVIDER_TIMEOUT"
                ))
        );
    }
}
