using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.Messaging.Events.Fraud;
using Shared.Messaging.Events.Payments;

namespace PaymentOrchestrator.Application.Sagas.Payment;

public sealed class PaymentStateMachine
    : MassTransitStateMachine<PaymentState>
{
    private readonly ILogger<PaymentStateMachine> _logger;

    // -------------------------
    // STATES
    // -------------------------
    public State FraudChecking { get; private set; } = default!;
    public State ProviderInitiated { get; private set; } = default!;
    public State Completed { get; private set; } = default!;
    public State Failed { get; private set; } = default!;

    // -------------------------
    // EVENTS
    // -------------------------
    public Event<PaymentCreatedEvent> PaymentCreated { get; private set; } = default!;
    public Event<FraudCheckCompletedEvent> FraudCheckCompleted { get; private set; } = default!;
    public Event<PaymentCompletedEvent> PaymentCompleted { get; private set; } = default!;
    public Event<PaymentFailedEvent> PaymentFailed { get; private set; } = default!;

    // -------------------------
    // SCHEDULE
    // -------------------------
    public Schedule<PaymentState, ProviderTimeoutExpiredEvent> ProviderTimeoutSchedule { get; private set; } = default!;

    private static readonly TimeSpan ProviderTimeout = TimeSpan.FromSeconds(45);

    public PaymentStateMachine(ILogger<PaymentStateMachine> logger)
    {
        _logger = logger;

        InstanceState(x => x.CurrentState);

        // -------------------------------------------------
        // CORRELATION
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

        Event(() => PaymentFailed,
            x => x.CorrelateById(ctx => ctx.Message.CorrelationId));

        // -------------------------------------------------
        // PROVIDER TIMEOUT SCHEDULE
        // -------------------------------------------------
        Schedule(() => ProviderTimeoutSchedule, x => x.ProviderTimeoutTokenId, s =>
        {
            s.Delay = ProviderTimeout;
            s.Received = e =>
                e.CorrelateById(ctx => ctx.Message.CorrelationId);
        });

        // -------------------------------------------------
        // INITIAL
        // -------------------------------------------------
        Initially(
            When(PaymentCreated)
                .Then(ctx =>
                {
                    ctx.Instance.CorrelationId = ctx.Message.CorrelationId;
                    ctx.Instance.PaymentId = ctx.Message.PaymentId;
                    ctx.Instance.MerchantId = ctx.Message.MerchantId;
                    ctx.Instance.Amount = ctx.Message.Amount;
                    ctx.Instance.Currency = ctx.Message.Currency;
                    ctx.Instance.CreatedAt = ctx.Message.CreatedAt;

                    using (BeginSagaScope(ctx))
                    {
                        _logger.LogInformation(
                            "Saga started | Amount={Amount} {Currency} | MerchantId={MerchantId}",
                            ctx.Instance.Amount,
                            ctx.Instance.Currency,
                            ctx.Instance.MerchantId);

                        LogTransition(nameof(Initial), nameof(FraudChecking));
                    }
                })
                .TransitionTo(FraudChecking)
                .Publish(ctx => new StartFraudCheckEvent(
                    ctx.Instance.CorrelationId,
                    ctx.Instance.PaymentId,
                    ctx.Instance.MerchantId,
                    ctx.Instance.Amount,
                    ctx.Instance.Currency))
        );

        // -------------------------------------------------
        // FRAUD CHECKING
        // -------------------------------------------------
        During(FraudChecking,
            When(FraudCheckCompleted)
                .Then(ctx =>
                {
                    using (BeginSagaScope(ctx))
                    {
                        _logger.LogInformation(
                            "Fraud check completed | Result={Result} | Reason={Reason}",
                            ctx.Message.IsFraud ? "FRAUD" : "CLEAN",
                            ctx.Message.Reason);
                    }
                })
                .IfElse(
                    ctx => ctx.Message.IsFraud,

                    // ---------- FRAUD → FAIL ----------
                    binder => binder
                        .Then(ctx =>
                        {
                            using (BeginSagaScope(ctx))
                            {
                                LogTransition(nameof(FraudChecking), nameof(Failed));
                            }
                        })
                        .TransitionTo(Failed)
                        .Publish(ctx => new PaymentFailedEvent(
                            ctx.Instance.CorrelationId,
                            ctx.Instance.PaymentId,
                            ctx.Message.Reason ?? "FRAUD_DETECTED"
                        )),

                    // ---------- CLEAN → PROVIDER ----------
                    binder => binder
                        .Then(ctx =>
                        {
                            using (BeginSagaScope(ctx))
                            {
                                LogTransition(nameof(FraudChecking), nameof(ProviderInitiated));
                            }
                        })
                        .Publish(ctx => new ProviderInitiationRequestedEvent(
                            ctx.Instance.CorrelationId,
                            ctx.Instance.PaymentId,
                            ctx.Instance.MerchantId,
                            ctx.Instance.Amount,
                            ctx.Instance.Currency,
                            ctx.Instance.ProviderName
                        ))
                        .Schedule(ProviderTimeoutSchedule, ctx => new ProviderTimeoutExpiredEvent
                        {
                            CorrelationId = ctx.Instance.CorrelationId
                        })
                        .TransitionTo(ProviderInitiated)
                )
        );

        // -------------------------------------------------
        // PROVIDER INITIATED
        // -------------------------------------------------
        During(ProviderInitiated,

            // ---------- PROVIDER SUCCESS ----------
            When(PaymentCompleted)
                .Unschedule(ProviderTimeoutSchedule)
                .Then(ctx =>
                {
                    using (BeginSagaScope(ctx))
                    {
                        LogTransition(nameof(ProviderInitiated), nameof(Completed));
                    }
                })
                .TransitionTo(Completed),

            // ---------- PROVIDER FAILURE ----------
            When(PaymentFailed)
                .Unschedule(ProviderTimeoutSchedule)
                .Then(ctx =>
                {
                    using (BeginSagaScope(ctx))
                    {
                        LogTransition(nameof(ProviderInitiated), nameof(Failed));
                    }
                })
                .TransitionTo(Failed),

            // ---------- PROVIDER TIMEOUT ----------
            When(ProviderTimeoutSchedule.Received)
                .Then(ctx =>
                {
                    using (BeginSagaScope(ctx))
                    {
                        _logger.LogError(
                            "Provider timeout after {TimeoutSeconds}s",
                            ProviderTimeout.TotalSeconds);

                        LogTransition(nameof(ProviderInitiated), nameof(Failed));
                    }
                })
                .TransitionTo(Failed)
                .Publish(ctx => new PaymentFailedEvent(
                    ctx.Instance.CorrelationId,
                    ctx.Instance.PaymentId,
                    "PROVIDER_TIMEOUT"
                ))
        );
    }

    // -------------------------------------------------
    // LOGGING HELPERS
    // -------------------------------------------------
    private IDisposable BeginSagaScope(BehaviorContext<PaymentState> ctx)
    {
        return _logger.BeginScope(new Dictionary<string, object>
        {
            ["PaymentId"] = ctx.Instance.PaymentId,
            ["CorrelationId"] = ctx.Instance.CorrelationId,
            ["SagaState"] = ctx.Instance.CurrentState
        });
    }

    private void LogTransition(string from, string to)
    {
        _logger.LogInformation(
            "Saga transition | {From} -> {To}",
            from,
            to);
    }
}
