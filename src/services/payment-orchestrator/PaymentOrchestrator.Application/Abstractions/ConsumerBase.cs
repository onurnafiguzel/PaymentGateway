using MassTransit;
using Microsoft.Extensions.Logging;

public abstract class ConsumerBase<TMessage>
    where TMessage : class
{
    protected readonly ILogger _logger;

    protected ConsumerBase(ILogger logger)
    {
        _logger = logger;
    }

    protected IDisposable BeginConsumeScope(ConsumeContext<TMessage> context, Guid? paymentId = null)
    {
        return _logger.BeginScope(new Dictionary<string, object>
        {
            ["PaymentId"] = paymentId,
            ["CorrelationId"] = context.CorrelationId,
            ["MessageId"] = context.MessageId,
            ["Consumer"] = GetType().Name
        });
    }
}
