using System.Diagnostics;
using MassTransit;
using Shared.Kernel.Tracing;

namespace PaymentOrchestrator.Infrastructure.Messaging;

public sealed class CorrelationIdPublishObserver : IPublishObserver
{
    public Task PrePublish<T>(PublishContext<T> context)
        where T : class
    {
        // Eğer zaten CorrelationId varsa dokunma
        if (context.CorrelationId.HasValue)
            return Task.CompletedTask;

        // OpenTelemetry Activity varsa oradan al
        var activity = Activity.Current;
        if (activity != null)
        {
            // TraceId (16 byte) → Guid’e map ediyoruz
            context.CorrelationId = activity.TraceId.ToGuid();
        }

        return Task.CompletedTask;
    }

    public Task PostPublish<T>(PublishContext<T> context)
        where T : class
        => Task.CompletedTask;

    public Task PublishFault<T>(
        PublishContext<T> context,
        Exception exception)
        where T : class
        => Task.CompletedTask;
}
