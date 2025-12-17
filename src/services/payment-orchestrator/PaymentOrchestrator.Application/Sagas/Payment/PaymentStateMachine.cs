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

    public Event<PaymentCreatedEvent> PaymentCreated { get; private set; } = default!;
    public Event<FraudCheckCompletedEvent> FraudCheckCompleted { get; private set; } = default!;
    public Event<PaymentCompletedEvent> PaymentCompleted { get; private set; } = default!;

    public Schedule<PaymentState, FraudTimeoutExpiredEvent> FraudTimeout { get; private set; } = default!;
    public Schedule<PaymentState, ProviderTimeoutExpiredEvent> ProviderTimeout { get; private set; } = default!;

    public PaymentStateMachine(ILogger<PaymentStateMachine> logger)
    {
        _logger = logger;

        InstanceState(x => x.CurrentState);

        // -------- CORRELATION --------
        Event(() => PaymentCreated, x =>
        {
            x.CorrelateById(ctx => ctx.CorrelationId!.Value);
            x.InsertOnInitial = true;
        });

        Event(() => FraudCheckCompleted,
            x => x.CorrelateById(ctx => ctx.CorrelationId!.Value));

        Event(() => PaymentCompleted,
            x => x.CorrelateById(ctx => ctx.CorrelationId!.Value));

        // -------- SCHEDULE CONFIG --------
        Schedule(() => FraudTimeout, x => x.FraudTimeoutTokenId, s =>
        {
            s.Delay = TimeSpan.FromMinutes(2);
            s.Received = e => e.CorrelateById(ctx => ctx.CorrelationId!.Value);
        });

        Schedule(() => ProviderTimeout, x => x.ProviderTimeoutTokenId, s =>
        {
            s.Delay = TimeSpan.FromMinutes(5);
            s.Received = e => e.CorrelateById(ctx => ctx.CorrelationId!.Value);
        });

        // -------- INITIAL --------
        Initially(
            When(PaymentCreated)
                .Then(ctx =>
                {
                    ctx.Instance.PaymentId = ctx.Message.PaymentId;
                    ctx.Instance.MerchantId = ctx.Message.MerchantId;
                    ctx.Instance.Amount = ctx.Message.Amount;
                    ctx.Instance.Currency = ctx.Message.Currency;
                    ctx.Instance.CreatedAt = ctx.Message.CreatedAt;

                    LogTransition(
                        ctx,
                        from: "INITIAL",
                        to: nameof(FraudChecking));
                })
                .Schedule(FraudTimeout, _ => new FraudTimeoutExpiredEvent())
                .TransitionTo(FraudChecking)
                .Publish(ctx => new StartFraudCheckEvent(
                    ctx.Instance.PaymentId,
                    ctx.Instance.MerchantId,
                    ctx.Instance.Amount,
                    ctx.Instance.Currency))
        );

        // -------- FRAUD CHECKING --------
        During(FraudChecking,
            When(FraudCheckCompleted)
                .Unschedule(FraudTimeout)
                .IfElse(
                    ctx => ctx.Message.IsFraud,
                    fraud => fraud
                        .Then(ctx =>
                        {
                            RecordDuration(ctx, "failed_fraud");
                            LogTransition(
                                ctx,
                                from: nameof(FraudChecking),
                                to: nameof(Completed));
                        })
                        .TransitionTo(Completed)
                        .Publish(ctx => new PaymentFailedEvent(
                            ctx.Instance.PaymentId,
                            ctx.Message.Reason ?? "FRAUD_DETECTED")),
                    clean => clean
                        .Then(ctx =>
                        {
                            LogTransition(
                                ctx,
                                from: nameof(FraudChecking),
                                to: nameof(ProviderInitiated));
                        })
                        .Schedule(ProviderTimeout, _ => new ProviderTimeoutExpiredEvent())
                        .TransitionTo(ProviderInitiated)
                        .Publish(ctx => new ProviderInitiationRequestedEvent(
                            ctx.Instance.PaymentId,
                            ctx.Instance.MerchantId,
                            ctx.Instance.Amount,
                            ctx.Instance.Currency,
                            "Iyzico"))
                )
        );

        // -------- PROVIDER --------
        During(ProviderInitiated,
            When(PaymentCompleted)
                .Then(ctx =>
                {
                    RecordDuration(ctx, "completed");
                    LogTransition(
                        ctx,
                        from: nameof(ProviderInitiated),
                        to: nameof(Completed));
                })
                .Unschedule(ProviderTimeout)
                .TransitionTo(Completed)
        );

        // -------- FRAUD TIMEOUT --------
        During(FraudChecking,
            When(FraudTimeout.Received)
                .Then(ctx =>
                {
                    RecordDuration(ctx, "timeout_fraud");
                    _logger.LogWarning(
                        "Fraud timeout | PaymentId={PaymentId} | CorrelationId={CorrelationId}",
                        ctx.Instance.PaymentId,
                        ctx.CorrelationId);
                })
                .TransitionTo(Completed)
                .Publish(ctx => new PaymentFailedEvent(
                    ctx.Instance.PaymentId,
                    "FRAUD_TIMEOUT"))
        );

        // -------- PROVIDER TIMEOUT --------
        During(ProviderInitiated,
            When(ProviderTimeout.Received)
                .Then(ctx =>
                {
                    RecordDuration(ctx, "timeout_provider");
                    _logger.LogWarning(
                        "Provider timeout | PaymentId={PaymentId} | CorrelationId={CorrelationId}",
                        ctx.Instance.PaymentId,
                        ctx.CorrelationId);
                })
                .TransitionTo(Completed)
                .Publish(ctx => new PaymentFailedEvent(
                    ctx.Instance.PaymentId,
                    "PROVIDER_TIMEOUT"))
        );
    }

    private void LogTransition(
        BehaviorContext<PaymentState> ctx,
        string from,
        string to)
    {
        _logger.LogInformation(
            "Payment saga state transition | PaymentId={PaymentId} | {FromState} -> {ToState} | CorrelationId={CorrelationId}",
            ctx.Instance.PaymentId,
            from,
            to,
            ctx.CorrelationId);
    }

    private void RecordDuration(
    BehaviorContext<PaymentState> ctx,
    string outcome)
    {
        var durationSeconds =
            (DateTime.UtcNow - ctx.Instance.CreatedAt).TotalSeconds;

        PaymentMetrics.PaymentDurationSeconds.Record(
            durationSeconds,
            new KeyValuePair<string, object?>("outcome", outcome),
            new KeyValuePair<string, object?>("payment_id", ctx.Instance.PaymentId)
        );
    }
}
