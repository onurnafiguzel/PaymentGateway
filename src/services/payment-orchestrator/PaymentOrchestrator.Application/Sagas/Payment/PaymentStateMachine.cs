using MassTransit;
using Microsoft.Extensions.Logging;
using PaymentOrchestrator.Application.Common.Observabilitiy;
using Shared.Messaging.Events.Fraud;
using Shared.Messaging.Events.Payments;

namespace PaymentOrchestrator.Application.Sagas.Payment;

public sealed class PaymentStateMachine
    : MassTransitStateMachine<PaymentState>
{
    private readonly ILogger<PaymentStateMachine> _logger;

    public State FraudChecking { get; private set; } = default!;
    public State ProviderInitiated { get; private set; } = default!;
    public State Completed { get; private set; } = default!;
    public State Failed { get; private set; } = default!;

    public Event<PaymentCreatedEvent> PaymentCreated { get; private set; } = default!;
    public Event<FraudCheckCompletedEvent> FraudCheckCompleted { get; private set; } = default!;
    public Event<PaymentCompletedEvent> PaymentCompleted { get; private set; } = default!;

    public PaymentStateMachine(ILogger<PaymentStateMachine> logger)
    {
        _logger = logger;

        InstanceState(x => x.CurrentState);

        // -------------------------------------------------
        // CORRELATION (🔥 EN KRİTİK DÜZELTME)
        // -------------------------------------------------
        Event(() => PaymentCreated, x =>
        {
            x.CorrelateById(ctx => ctx.Message.CorrelationId);
            x.SelectId(ctx => ctx.Message.CorrelationId);
            x.InsertOnInitial = true;
        });

        Event(() => FraudCheckCompleted,
            x => x.CorrelateById(ctx => ctx.Message.CorrelationId));

        Event(() => PaymentCompleted,
            x => x.CorrelateById(ctx => ctx.Message.CorrelationId));



        // -------------------------------------------------
        // INITIAL (Saga burada BAŞLAR)
        // -------------------------------------------------
        Initially(
            When(PaymentCreated)
                .Then(ctx =>
                {
                    _logger.LogInformation(
                        "Saga received PaymentCreatedEvent | PaymentId={PaymentId} | CorrelationId={CorrelationId}",
                        ctx.Message.PaymentId,
                        ctx.Message.CorrelationId);

                    ctx.Instance.CorrelationId = ctx.Message.CorrelationId;
                    ctx.Instance.PaymentId = ctx.Message.PaymentId;
                    ctx.Instance.MerchantId = ctx.Message.MerchantId;
                    ctx.Instance.Amount = ctx.Message.Amount;
                    ctx.Instance.Currency = ctx.Message.Currency;
                    ctx.Instance.CreatedAt = ctx.Message.CreatedAt;

                    LogTransition(ctx, "INITIAL", nameof(FraudChecking));
                })               
                .TransitionTo(FraudChecking)
                .Publish(ctx => new StartFraudCheckEvent(
                    ctx.Instance.CorrelationId,
                    ctx.Instance.PaymentId,
                    ctx.Instance.MerchantId,
                    ctx.Instance.Amount,
                    ctx.Instance.Currency))
        );

        During(FraudChecking,
            When(FraudCheckCompleted)
                .Then(ctx =>
                {
                    _logger.LogInformation(
                        "Saga received FraudCheckCompletedEvent | PaymentId={PaymentId} | IsFraud={IsFraud} | Reason={Reason} | CorrelationId={CorrelationId}",
                        ctx.Instance.PaymentId,
                        ctx.Message.IsFraud,
                        ctx.Message.Reason,
                        ctx.Instance.CorrelationId);
                })
                .IfElse(
                    ctx => ctx.Message.IsFraud,

                    // -------------------------
                    // FRAUD → FAIL
                    // -------------------------
                    binder => binder
                        .Then(ctx =>
                        {
                            LogTransition(ctx, nameof(FraudChecking), nameof(Failed));
                        })
                        .TransitionTo(Failed)
                        .Publish(ctx => new PaymentFailedEvent(
                            ctx.Instance.CorrelationId,
                            ctx.Instance.PaymentId,
                            ctx.Message.Reason ?? "FRAUD_DETECTED"
                        )),

                    // -------------------------
                    // CLEAN → PROVIDER
                    // -------------------------
                    binder => binder
                        .Then(ctx =>
                        {
                            LogTransition(ctx, nameof(FraudChecking), nameof(ProviderInitiated));                           
                        })
                        .Publish(ctx => new ProviderInitiationRequestedEvent(
                            ctx.Instance.CorrelationId,
                            ctx.Instance.PaymentId,
                            ctx.Instance.MerchantId,
                            ctx.Instance.Amount,
                            ctx.Instance.Currency,
                            ctx.Instance.ProviderName
                        ))
                        .TransitionTo(ProviderInitiated)
                )
        );
    }

    private void LogTransition(
        BehaviorContext<PaymentState> ctx,
        string from,
        string to)
    {
        _logger.LogInformation(
            "Payment saga transition | PaymentId={PaymentId} | {From} -> {To} | CorrelationId={CorrelationId}",
            ctx.Instance.PaymentId,
            from,
            to,
            ctx.Instance.CorrelationId);
    }
}
