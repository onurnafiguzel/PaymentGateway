using System.Diagnostics;
using MassTransit;

namespace PaymentOrchestrator.Application.Observability;

public sealed class ConsumerObservabilityFilter<T>
    : IFilter<ConsumeContext<T>>
    where T : class
{
    public async Task Send(
        ConsumeContext<T> context,
        IPipe<ConsumeContext<T>> next)
    {
        using var activity = ConsumerActivitySource.Instance.StartActivity(
            $"{typeof(T).Name}.Consume",
            ActivityKind.Consumer);

        if (activity != null)
        {
            // Correlation
            if (context.CorrelationId.HasValue)
                activity.SetTag("correlation.id", context.CorrelationId.ToString());

            // Messaging
            activity.SetTag("messaging.system", "rabbitmq");
            activity.SetTag("messaging.operation", "consume");
            activity.SetTag("messaging.message_id", context.MessageId?.ToString());
            activity.SetTag("messaging.destination", context.DestinationAddress?.ToString());

            // Optional: headers
            if (context.Headers.TryGetHeader("PaymentId", out var paymentId))
                activity.SetTag("payment.id", paymentId?.ToString());
        }

        await next.Send(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateFilterScope("consumer-observability-filter");
    }
}
