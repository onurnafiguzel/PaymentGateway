using MassTransit;
using Microsoft.Extensions.Logging;

namespace PaymentOrchestrator.Application.Consumers.Monitoring;

public sealed class FaultConsumer<T> : IConsumer<Fault<T>>
    where T : class
{
    private readonly ILogger<FaultConsumer<T>> _logger;

    public FaultConsumer(ILogger<FaultConsumer<T>> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<Fault<T>> context)
    {
        _logger.LogError(
            "Message faulted | MessageType={MessageType} | CorrelationId={CorrelationId} | Exception={ExceptionType} | MessageId={MessageId}",
            typeof(T).Name,
            context.CorrelationId,
            context.MessageId,
            context.Message.Exceptions?.FirstOrDefault()?.ExceptionType,
            context.Message.Exceptions?.FirstOrDefault()?.Message
        );

        return Task.CompletedTask;
    }
}
